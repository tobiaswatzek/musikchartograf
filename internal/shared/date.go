package shared

import "time"

// Calculates the first monday of a week in the given year.
// Adapted from https://stackoverflow.com/a/52303730
func WeekStart(year int, week int, loc *time.Location) time.Time {
	// Start from the middle of the year:
	t := time.Date(year, time.July, 1, 0, 0, 0, 0, loc)

	// Roll back to Monday:
	if wd := t.Weekday(); wd == time.Sunday {
		t = t.AddDate(0, 0, -6)
	} else {
		t = t.AddDate(0, 0, -int(wd)+1)
	}

	// Difference in weeks:
	_, w := t.ISOWeek()
	t = t.AddDate(0, 0, (week-w)*7)

	return t
}
