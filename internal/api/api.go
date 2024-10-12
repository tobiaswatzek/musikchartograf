package api

import (
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"net/url"
	"strconv"
	"strings"
	"time"

	"musikchartograf/internal/shared"
)

type ApiValidationError struct {
	Reason string
}

func (m *ApiValidationError) Error() string {
	return fmt.Sprintf("Validation failed: %s", m.Reason)
}

func newValidationError(reason string) *ApiValidationError {
	return &ApiValidationError{Reason: reason}
}

type ApiClient interface {
	// Get the tracks played by a last.fm user between from and to.
	//
	// limit: Number of tracks to return at most. Maximum is 200.
	GetRecentTracksOfUser(user string, from time.Time, to time.Time, limit int, page int) (*RecentTracksResponse, error)
}

type apiClient struct {
	baseURL *url.URL
	apiKey  string
}

func NewApiClient(baseURL string, apiKey string) (ApiClient, error) {
	u, err := url.ParseRequestURI(baseURL)
	if err != nil {
		return nil, err
	}

	trimmedApiKey := strings.TrimSpace(apiKey)
	if trimmedApiKey == "" {
		return nil, newValidationError("API key cannot be empty or whitespace only")
	}

	return apiClient{baseURL: u, apiKey: trimmedApiKey}, nil
}

type RecentTracksResponse struct {
	User       string
	Page       int
	PerPage    int
	TotalPages int
	Total      int
	NowPlaying shared.Option[RecentTrackNowPlaying]
	Tracks     []RecentTrack
}

type RecentTrackNowPlaying struct {
	Name   string
	Artist string
	Album  string
}

type RecentTrack struct {
	Name   string
	Artist string
	Album  string
	Date   time.Time
}

type lastfmRecentTracksResponse struct {
	RecentTracks struct {
		Track []struct {
			Artist struct {
				Mbid string `json:"mbid"`
				Text string `json:"#text"`
			} `json:"artist"`
			Streamable string `json:"streamable"`
			Image      []struct {
				Size string `json:"size"`
				Text string `json:"#text"`
			} `json:"image"`
			Mbid  string `json:"mbid"`
			Album struct {
				Mbid string `json:"mbid"`
				Text string `json:"#text"`
			} `json:"album"`
			Name string `json:"name"`
			Attr *struct {
				Nowplaying string `json:"nowplaying"`
			} `json:"@attr,omitempty"`
			URL  string `json:"url"`
			Date *struct {
				Uts  string `json:"uts"`
				Text string `json:"#text"`
			} `json:"date,omitempty"`
		} `json:"track"`
		Attr struct {
			User       string `json:"user"`
			TotalPages string `json:"totalPages"`
			Page       string `json:"page"`
			PerPage    string `json:"perPage"`
			Total      string `json:"total"`
		} `json:"@attr"`
	} `json:"recenttracks"`
}

func (ac apiClient) GetRecentTracksOfUser(user string, from time.Time, to time.Time, limit int, page int) (*RecentTracksResponse, error) {
	trimmedUser := strings.TrimSpace(user)
	if trimmedUser == "" {
		return nil, newValidationError("user cannot be empty or whitespace only")
	}

	if from.IsZero() {
		return nil, newValidationError("from timestamp cannot have zero value")
	}

	if to.IsZero() {
		return nil, newValidationError("to timestamp cannot have zero value")
	}

	if from.After(to) {
		return nil, newValidationError("from timestamp cannot be before to timestamp")
	}

	if limit < 0 || limit > 200 {
		return nil, newValidationError("limit must be between 1 and 200")
	}

	if page < 0 {
		return nil, newValidationError("page must be greater than 0")
	}

	url := *ac.baseURL

	q := ac.baseURL.Query()
	q.Set("format", "json")
	q.Set("method", "user.getRecentTracks")
	q.Set("api_key", ac.apiKey)
	q.Set("user", user)
	q.Set("page", strconv.FormatInt(int64(page), 10))
	q.Set("limit", strconv.FormatInt(int64(limit), 10))
	q.Set("from", strconv.FormatInt(from.UTC().Unix(), 10))
	q.Set("to", strconv.FormatInt(to.UTC().Unix(), 10))

	url.RawQuery = q.Encode()

	resp, err := http.Get(url.String())

	if err != nil {
		return nil, err
	}

	defer resp.Body.Close()

	var jsonResp lastfmRecentTracksResponse
	err = json.NewDecoder(resp.Body).Decode(&jsonResp)
	if err != nil {
		return nil, err
	}

	recentTracksResponse, err := jsonResponseToRecentTracksResponse(&jsonResp)

	return recentTracksResponse, nil
}

func jsonResponseToRecentTracksResponse(jsonResp *lastfmRecentTracksResponse) (*RecentTracksResponse, error) {
	nowPlaying := shared.None[RecentTrackNowPlaying]()
	tracks := make([]RecentTrack, 0, len(jsonResp.RecentTracks.Track))
	for _, track := range jsonResp.RecentTracks.Track {
		if track.Attr != nil {
			isNowPlaying, err := strconv.ParseBool(track.Attr.Nowplaying)
			if err != nil {
				return nil, err
			}
			if isNowPlaying {
				if !nowPlaying.IsEmpty() {
					return nil, errors.New("found multiple now playing tracks")
				}
				nowPlaying = shared.Some(RecentTrackNowPlaying{Name: track.Name, Artist: track.Artist.Text, Album: track.Album.Text})
			}
		}

		if track.Date == nil {
			return nil, errors.New("track is expected to have date")
		}

		trackDateInt, err := strconv.ParseInt(track.Date.Uts, 10, 64)
		if err != nil {
			return nil, err
		}
		trackDate := time.Unix(trackDateInt, 0)

		// todo: maybe add validation for strings, date?

		t := RecentTrack{
			Name:   track.Name,
			Artist: track.Artist.Text,
			Album:  track.Album.Text,
			Date:   trackDate,
		}

		tracks = append(tracks, t)
	}

	page, err := strconv.Atoi(jsonResp.RecentTracks.Attr.Page)
	if err != nil {
		return nil, err
	}
	perPage, err := strconv.Atoi(jsonResp.RecentTracks.Attr.PerPage)
	if err != nil {
		return nil, err
	}
	totalPages, err := strconv.Atoi(jsonResp.RecentTracks.Attr.TotalPages)
	if err != nil {
		return nil, err
	}
	total, err := strconv.Atoi(jsonResp.RecentTracks.Attr.Total)
	if err != nil {
		return nil, err
	}

	recentTracksResponse := RecentTracksResponse{
		User:       jsonResp.RecentTracks.Attr.User,
		Page:       page,
		PerPage:    perPage,
		TotalPages: totalPages,
		Total:      total,
		NowPlaying: nowPlaying,
		Tracks:     tracks,
	}

	return &recentTracksResponse, nil
}
