package shared

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func Test_Some(t *testing.T) {
	actual := Some(42)

	assert.Equal(t, Option[int]{value: 42, isFull: true}, actual)
}

func Test_None(t *testing.T) {
	actual := None[int]()

	assert.Equal(t, Option[int]{value: 0, isFull: false}, actual)
}

func Test_Get(t *testing.T) {
	actual := Some("foo")

	assert.Equal(t, "foo", actual.Get())
}

func Test_Get_Panics(t *testing.T) {
	actual := None[int]()

	assert.Panics(t, func() { actual.Get() })
}

func Test_GetOrElse(t *testing.T) {
	testCases := []struct {
		desc     string
		option   Option[int]
		other    int
		expected int
	}{
		{
			desc:     "Some",
			option:   Some(42),
			other:    300,
			expected: 42,
		},
		{
			desc:     "None",
			option:   None[int](),
			other:    300,
			expected: 300,
		},
	}
	for _, tC := range testCases {
		t.Run(tC.desc, func(t *testing.T) {
			actual := tC.option.GetOrElse(tC.other)

			assert.Equal(t, tC.expected, actual)
		})
	}
}

func Test_IsEmpty(t *testing.T) {
	testCases := []struct {
		desc     string
		option   Option[int]
		expected bool
	}{
		{
			desc:     "Some",
			option:   Some(42),
			expected: false,
		},
		{
			desc:     "None",
			option:   None[int](),
			expected: true,
		},
	}
	for _, tC := range testCases {
		t.Run(tC.desc, func(t *testing.T) {
			actual := tC.option.IsEmpty()

			assert.Equal(t, tC.expected, actual)
		})
	}
}

func Test_String(t *testing.T) {
	testCases := []struct {
		desc     string
		option   Option[int]
		expected string
	}{
		{
			desc:     "Some",
			option:   Some(42),
			expected: "Some(42)",
		},
		{
			desc:     "None",
			option:   None[int](),
			expected: "None",
		},
	}
	for _, tC := range testCases {
		t.Run(tC.desc, func(t *testing.T) {
			actual := tC.option.String()

			assert.Equal(t, tC.expected, actual)
		})
	}
}
