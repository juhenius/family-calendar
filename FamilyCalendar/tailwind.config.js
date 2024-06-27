/** @type {import("tailwindcss").Config} */
module.exports = {
  content: ["./Pages/**/*.cshtml"],
  theme: {
    extend: {
      animation: {
        "card-flash": "flash 3s linear 1",
      },
      keyframes: {
        "flash": {
          "0%": { backgroundColor: "#ffffbb" },
          "100%": { backgroundColor: "inherit" },
        }
      }
    },
  },
  plugins: [],
  safelist: [{ pattern: /grid-cols-.*/ }]
}

