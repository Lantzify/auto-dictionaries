/// <binding ProjectOpened='default' />
//
const { watch, src, dest } = require("gulp");
var config = require("./paths.json");

function copy(path, baseFolder, targets) {

    targets.forEach(target => {
        console.log("copy: \x1b[36m%s\x1b[0m %s", path, target);  

        src(path, { base: baseFolder }).pipe(dest(target));
    });
}

function watchAppPlugins() {

    console.log()
    console.log("Watching : " + config.source);

    config.sites.forEach(dest => {
        console.log("Target   : " + dest);
    });
   
    watch(config.source, { ignoreInitial: false })
        .on("change", function (path, stats) {
            copy(path, config.source, config.sites)
        })
        .on("add", function (path, stats) {
            copy(path, config.source, config.sites)
        });
}

exports.default = function () {
    watchAppPlugins();
};