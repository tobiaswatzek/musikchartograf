package main

import (
	"fmt"
	"os"
	"strings"
	"time"

	"musikchartograf/internal/api"
	"musikchartograf/internal/domain"
)

func main() {
	apiKeyFile := strings.TrimSpace(os.Args[1])
	user := strings.TrimSpace(os.Args[2])
	apiKey := readApiKeyFromFile(apiKeyFile)
	apiClient, err := api.NewApiClient("https://ws.audioscrobbler.com/2.0/", apiKey)
	if err != nil {
		panic(err)
	}

	resp, err := apiClient.GetRecentTracksOfUser(user, time.Date(2024, time.September, 23, 0, 0, 0, 0, time.Local), time.Date(2024, time.September, 29, 23, 59, 59, 0, time.Local), 200, 1)

	if err != nil {
		panic(err)
	}

	playedTracks := make([]domain.PlayedTrack, 0, len(resp.Tracks))
	for _, t := range resp.Tracks {
		playedTracks = append(playedTracks, domain.PlayedTrack{Name: t.Name, Artist: t.Artist, Album: t.Album, Date: t.Date})
	}

	charts := domain.CalculateWeeklyCharts(100, 2024, 39, playedTracks)

	fmt.Printf("%+v\n", charts)
}

func readApiKeyFromFile(apiKeyFile string) string {
	b, err := os.ReadFile(apiKeyFile)
	if err != nil {
		panic(err)
	}
	apiKey := strings.TrimSpace(string(b))
	return apiKey
}
