/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // Primary brand: medical emerald green
        primary: {
          50:  "#f0fdf4",
          100: "#dcfce7",
          200: "#bbf7d0",
          300: "#86efac",
          400: "#4ade80",
          500: "#22c55e",
          600: "#16a34a",
          700: "#15803d",
          800: "#166534",
          900: "#14532d",
          950: "#052e16",
        },
        // Status / medical semantic colours
        medical: {
          green:  "#16a34a",
          red:    "#dc2626",
          yellow: "#d97706",
          blue:   "#2563eb",
          teal:   "#0d9488",
          purple: "#7c3aed",
        },
        // Neutral surface palette (slightly warm)
        surface: {
          50:  "#fafaf9",
          100: "#f5f5f4",
          200: "#e7e5e4",
          300: "#d6d3d1",
          400: "#a8a29e",
          500: "#78716c",
          600: "#57534e",
          700: "#44403c",
          800: "#292524",
          900: "#1c1917",
        },
      },
      borderRadius: {
        sm:   "0.375rem",   // 6 px
        DEFAULT: "0.5rem",  // 8 px
        md:   "0.625rem",   // 10 px
        lg:   "0.875rem",   // 14 px
        xl:   "1rem",       // 16 px
        "2xl":"1.25rem",    // 20 px
        "3xl":"1.75rem",    // 28 px
        "4xl":"2.5rem",     // 40 px
      },
      spacing: {
        "18":  "4.5rem",
        "22":  "5.5rem",
        "72":  "18rem",
        "80":  "20rem",
        "88":  "22rem",
        "96":  "24rem",
        "128": "32rem",
      },
      fontFamily: {
        sans: ["Inter", "ui-sans-serif", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Roboto", "sans-serif"],
        mono: ["JetBrains Mono", "Menlo", "monospace"],
      },
      fontSize: {
        "2xs": ["0.65rem",  { lineHeight: "1rem" }],
        xs:    ["0.75rem",  { lineHeight: "1rem" }],
        sm:    ["0.875rem", { lineHeight: "1.25rem" }],
        base:  ["1rem",     { lineHeight: "1.6rem" }],
        lg:    ["1.125rem", { lineHeight: "1.75rem" }],
        xl:    ["1.25rem",  { lineHeight: "1.75rem" }],
        "2xl": ["1.5rem",   { lineHeight: "2rem" }],
        "3xl": ["1.875rem", { lineHeight: "2.25rem" }],
      },
      boxShadow: {
        "xs":    "0 1px 2px 0 rgb(0 0 0 / 0.04)",
        "sm":    "0 1px 3px 0 rgb(0 0 0 / 0.06), 0 1px 2px -1px rgb(0 0 0 / 0.04)",
        "DEFAULT":"0 2px 8px -1px rgb(0 0 0 / 0.08), 0 2px 4px -2px rgb(0 0 0 / 0.05)",
        "md":    "0 4px 12px -2px rgb(0 0 0 / 0.10), 0 2px 6px -2px rgb(0 0 0 / 0.06)",
        "lg":    "0 10px 24px -4px rgb(0 0 0 / 0.10), 0 4px 10px -3px rgb(0 0 0 / 0.06)",
        "xl":    "0 20px 40px -8px rgb(0 0 0 / 0.12), 0 8px 16px -6px rgb(0 0 0 / 0.07)",
        "2xl":   "0 32px 64px -12px rgb(0 0 0 / 0.15)",
        "glow":  "0 0 0 3px rgb(34 197 94 / 0.18)",
        "glow-lg":"0 0 24px 4px rgb(34 197 94 / 0.14)",
        "inner-sm":"inset 0 1px 2px 0 rgb(0 0 0 / 0.05)",
        "none":  "none",
      },
      keyframes: {
        "fade-in": {
          "0%":   { opacity: "0" },
          "100%": { opacity: "1" },
        },
        "fade-in-up": {
          "0%":   { opacity: "0", transform: "translateY(16px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
        "fade-in-down": {
          "0%":   { opacity: "0", transform: "translateY(-16px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
        "scale-in": {
          "0%":   { opacity: "0", transform: "scale(0.94)" },
          "100%": { opacity: "1", transform: "scale(1)" },
        },
        "slide-in-right": {
          "0%":   { opacity: "0", transform: "translateX(-16px)" },
          "100%": { opacity: "1", transform: "translateX(0)" },
        },
        "slide-out-left": {
          "0%":   { opacity: "1", transform: "translateX(0)" },
          "100%": { opacity: "0", transform: "translateX(-16px)" },
        },
        "shimmer": {
          "0%":   { backgroundPosition: "-200% 0" },
          "100%": { backgroundPosition: "200% 0" },
        },
        "pulse-soft": {
          "0%, 100%": { opacity: "1" },
          "50%":      { opacity: "0.6" },
        },
        "bounce-subtle": {
          "0%, 100%": { transform: "translateY(0)" },
          "50%":      { transform: "translateY(-4px)" },
        },
        "spin-slow": {
          "0%":   { transform: "rotate(0deg)" },
          "100%": { transform: "rotate(360deg)" },
        },
        "count-up": {
          "0%":   { opacity: "0", transform: "translateY(8px) scale(0.95)" },
          "100%": { opacity: "1", transform: "translateY(0) scale(1)" },
        },
      },
      animation: {
        "fade-in":       "fade-in 250ms cubic-bezier(0.16, 1, 0.3, 1)",
        "fade-in-up":    "fade-in-up 350ms cubic-bezier(0.16, 1, 0.3, 1)",
        "fade-in-down":  "fade-in-down 300ms cubic-bezier(0.16, 1, 0.3, 1)",
        "scale-in":      "scale-in 280ms cubic-bezier(0.34, 1.56, 0.64, 1)",
        "slide-in-right":"slide-in-right 300ms cubic-bezier(0.16, 1, 0.3, 1)",
        "shimmer":       "shimmer 2s linear infinite",
        "pulse-soft":    "pulse-soft 2.5s ease-in-out infinite",
        "bounce-subtle": "bounce-subtle 2s ease-in-out infinite",
        "spin-slow":     "spin-slow 2s linear infinite",
        "count-up":      "count-up 400ms cubic-bezier(0.34, 1.56, 0.64, 1)",
      },
      transitionTimingFunction: {
        "spring":  "cubic-bezier(0.34, 1.56, 0.64, 1)",
        "smooth":  "cubic-bezier(0.16, 1, 0.3, 1)",
        "snappy":  "cubic-bezier(0.4, 0, 0.2, 1)",
      },
      transitionDuration: {
        "150": "150ms",
        "200": "200ms",
        "250": "250ms",
        "300": "300ms",
        "400": "400ms",
        "500": "500ms",
      },
      backgroundImage: {
        "gradient-radial": "radial-gradient(var(--tw-gradient-stops))",
        "shimmer-gradient": "linear-gradient(90deg, transparent 0%, rgb(255 255 255 / 0.12) 50%, transparent 100%)",
      },
    },
  },
  plugins: [],
  darkMode: "class",
};
