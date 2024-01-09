const path = require("path");

module.exports = {
  outputDir: path.resolve(
    __dirname,
    process.env.NODE_ENV === "production"
      ? "../src/Elmah.AspNetCore/wwwroot"
      : "dist"
  ),
  publicPath: "ELMAH_ROOT",
  pages: {
    index: {
      entry: "src/main.js",
      title: "Elmah",
    },
  },
};
