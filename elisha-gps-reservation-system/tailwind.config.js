/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './**/*.html',
    './**/*.razor',
    './**/*.cshtml',
  ],
  theme: {
    extend: {
      fontFamily: {
        'jakarta': ['Plus Jakarta Sans', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
