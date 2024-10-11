package domain

import (
	"cmp"
	"fmt"
	"maps"
	"slices"
	"time"
)

type PlayedTrack struct {
	Name   string
	Artist string
	Album  string
	Date   time.Time
}

func (pt PlayedTrack) id() string {
	return fmt.Sprintf("%s_%s_%s", pt.Name, pt.Artist, pt.Album)
}

type RankedTrack struct {
	Points int
	Plays  int
	Name   string
	Artist string
	Album  string
}

func (rt RankedTrack) Id() string {
	return fmt.Sprintf("%s_%s_%s", rt.Name, rt.Artist, rt.Album)
}

func CalculateWeeklyCharts(top int, year int, week int, tracks []PlayedTrack) []*RankedTrack {
	rankedMap := make(map[string]*RankedTrack, 0)

	for _, t := range tracks {
		if y, w := t.Date.ISOWeek(); y != year || w != week {
			continue
		}

		if rankedMap[t.id()] == nil {
			rankedMap[t.id()] = &RankedTrack{
				Points: 0,
				Plays:  0,
				Name:   t.Name,
				Artist: t.Artist,
				Album:  t.Album,
			}
		}
		rankedMap[t.id()].Plays++
	}

	if len(rankedMap) == 0 {
		return []*RankedTrack{}
	}

	sortedTracks := slices.Collect(maps.Values(rankedMap))

	slices.SortFunc(sortedTracks, func(a *RankedTrack, b *RankedTrack) int {
		return cmp.Compare(b.Plays, a.Plays)
	})

	maxElements := len(sortedTracks)
	if top < maxElements {
		maxElements = top
	}

	rankedTracks := sortedTracks[:maxElements]

	currentPoints := maxElements
	previousPlays := rankedTracks[0].Plays
	for _, t := range rankedTracks {
		if previousPlays > t.Plays {
			currentPoints--
		}
		t.Points = currentPoints
		previousPlays = t.Plays
	}

	return rankedTracks
}
