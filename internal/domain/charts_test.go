package domain

import (
	"math/rand/v2"
	"slices"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func Test_CalculateWeeklyCharts_SmallerTop(t *testing.T) {
	from := time.Date(2024, time.September, 30, 0, 0, 0, 0, time.Local)
	to := time.Date(2024, time.October, 6, 23, 59, 59, 0, time.Local)
	firstPlace := generateTracks(20,
		from,
		to,
		"Respect",
		"Aretha Franklin",
		"Otis Blue: Otis Redding Sings Soul")
	secondPlace := generateTracks(19,
		from,
		to,
		"Fight the Power",
		"Public Enemy",
		"Fear of a Black Planet")
	thirdPlace := generateTracks(2,
		from,
		to,
		"A Change Is Gonna Come",
		"Sam Cooke",
		"Ain’t That Good News")
	alsoThirdPlace := generateTracks(2,
		from,
		to,
		"Like a Rolling Stone",
		"Bob Dyllan",
		"Highway 61 Revisited")
	notPartOfTheCharts := generateTracks(1,
		from,
		to,
		"Smells Like Teen Spirit",
		"Nirvana",
		"Nevermind")

	charts := CalculateWeeklyCharts(4, 2024, 40, slices.Concat(firstPlace, secondPlace, thirdPlace, alsoThirdPlace, notPartOfTheCharts))

	require.Len(t, charts, 4)
	assert.Contains(t, charts, &RankedTrack{
		Points: 4,
		Plays:  20,
		Name:   "Respect",
		Artist: "Aretha Franklin",
		Album:  "Otis Blue: Otis Redding Sings Soul",
	})
	assert.Contains(t, charts, &RankedTrack{
		Points: 3,
		Plays:  19,
		Name:   "Fight the Power",
		Artist: "Public Enemy",
		Album:  "Fear of a Black Planet",
	})
	assert.Contains(t, charts, &RankedTrack{
		Points: 2,
		Plays:  2,
		Name:   "A Change Is Gonna Come",
		Artist: "Sam Cooke",
		Album:  "Ain’t That Good News",
	})
	assert.Contains(t, charts, &RankedTrack{
		Points: 2,
		Plays:  2,
		Name:   "Like a Rolling Stone",
		Artist: "Bob Dyllan",
		Album:  "Highway 61 Revisited",
	})
}

func Test_CalculateWeeklyCharts_BiggerTop(t *testing.T) {
	from := time.Date(2024, time.September, 30, 0, 0, 0, 0, time.Local)
	to := time.Date(2024, time.October, 6, 23, 59, 59, 0, time.Local)
	firstPlace := generateTracks(20,
		from,
		to,
		"Respect",
		"Aretha Franklin",
		"Otis Blue: Otis Redding Sings Soul")
	secondPlace := generateTracks(19,
		from,
		to,
		"Fight the Power",
		"Public Enemy",
		"Fear of a Black Planet")
	thirdPlace := generateTracks(2,
		from,
		to,
		"A Change Is Gonna Come",
		"Sam Cooke",
		"Ain’t That Good News")

	charts := CalculateWeeklyCharts(10, 2024, 40, slices.Concat(firstPlace, secondPlace, thirdPlace))

	require.Len(t, charts, 3)
	assert.Contains(t, charts, &RankedTrack{
		Points: 3,
		Plays:  20,
		Name:   "Respect",
		Artist: "Aretha Franklin",
		Album:  "Otis Blue: Otis Redding Sings Soul",
	})
	assert.Contains(t, charts, &RankedTrack{
		Points: 2,
		Plays:  19,
		Name:   "Fight the Power",
		Artist: "Public Enemy",
		Album:  "Fear of a Black Planet",
	})
	assert.Contains(t, charts, &RankedTrack{
		Points: 1,
		Plays:  2,
		Name:   "A Change Is Gonna Come",
		Artist: "Sam Cooke",
		Album:  "Ain’t That Good News",
	})
}

func Test_CalculateWeeklyCharts_NoneInWeek(t *testing.T) {
	from := time.Date(2024, time.October, 7, 0, 0, 0, 0, time.Local)
	to := time.Date(2024, time.October, 13, 23, 59, 59, 0, time.Local)
	respect := generateTracks(20,
		from,
		to,
		"Respect",
		"Aretha Franklin",
		"Otis Blue: Otis Redding Sings Soul")
	fight := generateTracks(19,
		from,
		to,
		"Fight the Power",
		"Public Enemy",
		"Fear of a Black Planet")
	change := generateTracks(2,
		from,
		to,
		"A Change Is Gonna Come",
		"Sam Cooke",
		"Ain’t That Good News")

	charts := CalculateWeeklyCharts(4, 2024, 40, slices.Concat(respect, fight, change))

	require.Empty(t, charts)
}

func generateTracks(count int, from time.Time, to time.Time, name string, artist string, album string) []PlayedTrack {
	tracks := make([]PlayedTrack, 0, count)
	for i := 0; i < count; i++ {
		d := generateDateBetween(from, to)
		tracks = append(tracks, PlayedTrack{
			Name:   name,
			Artist: artist,
			Album:  album,
			Date:   d,
		})
	}

	return tracks
}

func generateDateBetween(from time.Time, to time.Time) time.Time {
	duration := to.Sub(from)
	add := time.Duration(rand.Int64N(int64(duration.Minutes()))) * time.Minute
	date := from.Add(add)

	return date
}
