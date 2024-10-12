package shared

import (
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
)

func Test_WeekStart(t *testing.T) {
	testCases := []struct {
		desc     string
		year     int
		week     int
		expected time.Time
	}{
		{
			desc:     "2022 W52",
			year:     2022,
			week:     52,
			expected: time.Date(2022, 12, 26, 0, 0, 0, 0, time.UTC),
		},
		{
			desc:     "2023 W01",
			year:     2023,
			week:     01,
			expected: time.Date(2023, 01, 02, 0, 0, 0, 0, time.UTC),
		},
		{
			desc:     "2025 W01",
			year:     2025,
			week:     01,
			expected: time.Date(2024, 12, 30, 0, 0, 0, 0, time.UTC),
		},
		{
			desc:     "2018 W27",
			year:     2018,
			week:     27,
			expected: time.Date(2018, 07, 02, 0, 0, 0, 0, time.UTC),
		},
	}
	for _, tC := range testCases {
		t.Run(tC.desc, func(t *testing.T) {
			actual := WeekStart(tC.year, tC.week, time.UTC)

			assert.Equal(t, tC.expected, actual)
		})
	}
}
