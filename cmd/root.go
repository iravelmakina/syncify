package cmd

import (
	"os"

	"github.com/rs/zerolog"
	"github.com/rs/zerolog/log"
	"github.com/rs/zerolog/pkgerrors"
	"github.com/spf13/cobra"
)

var (
	logLevel         string
	enablePrettyLogs bool
)

var rootCmd = &cobra.Command{
	Use:   "syncify",
	Short: "Keeps availability accurate across multiple calendar platforms",
	PersistentPreRun: func(_ *cobra.Command, _ []string) {
		zerolog.ErrorStackMarshaler = pkgerrors.MarshalStack

		if enablePrettyLogs {
			log.Logger = log.Output(zerolog.ConsoleWriter{
				Out: os.Stderr,
			})
			log.Info().Msg("Enabled human readable logging!")
		}

		validLogLevel, err := zerolog.ParseLevel(logLevel)
		if err != nil {
			log.Fatal().Err(err).Msg("Failed to parse log level")
		}

		log.Logger = log.Level(validLogLevel).With().Stack().Logger()
		log.Info().Msgf("Log level is set to %q", logLevel)

		zerolog.DefaultContextLogger = &log.Logger
	},
}

func Execute() {
	if err := rootCmd.Execute(); err != nil {
		os.Exit(1)
	}
}

func init() {
	rootCmd.PersistentFlags().StringVar(&logLevel, "log-level", "info", "logging level")
	rootCmd.PersistentFlags().BoolVar(&enablePrettyLogs, "pretty-logs", false, "human readable logs")
}
