package main

import (
	"fmt"
	"os"
	"strings"
	"time"

	"music-charts-tui/internal/api"
)

func main() {
	apiKeyFile := strings.TrimSpace(os.Args[1])
	user := strings.TrimSpace(os.Args[2])
	apiKey := readApiKeyFromFile(apiKeyFile)
	apiClient, err := api.NewApiClient("https://ws.audioscrobbler.com/2.0/", apiKey)
	if err != nil {
		panic(err)
	}

	resp, err := apiClient.GetRecentTracksOfUser(user, time.Now().Add(-24*time.Hour), time.Now(), 50, 1)

	if err != nil {
		panic(err)
	}

	fmt.Printf("%+v\n", resp)
}

func readApiKeyFromFile(apiKeyFile string) string {
	b, err := os.ReadFile(apiKeyFile)
	if err != nil {
		panic(err)
	}
	apiKey := strings.TrimSpace(string(b))
	return apiKey
}
