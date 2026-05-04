window.ff = window.ff || {};

/** Match MudBreakPoint.Md (960px) — used to open sidebar by default on desktop only. */
window.ffIsWideLayout = function () {
    return window.matchMedia('(min-width: 960px)').matches;
};

/**
 * MudBlazor.min.js exposes window.mudThemeProvider.watchDarkMode / stopWatchingDarkMode.
 * Some MudBlazor.dll builds invoke JS as global identifiers watchDarkThemeMedia / stopWatchingDarkThemeMedia
 * (Blazor looks these up on window, NOT on mudThemeProvider). Define globals that delegate to mudThemeProvider.
 */
(function () {
    function bridge() {
        var mtp = window.mudThemeProvider;
        if (!mtp || typeof mtp.watchDarkMode !== 'function') return;

        function watch(dotNetRef) {
            return mtp.watchDarkMode(dotNetRef);
        }

        function stop() {
            return mtp.stopWatchingDarkMode();
        }

        if (typeof window.watchDarkThemeMedia !== 'function')
            window.watchDarkThemeMedia = watch;

        if (typeof window.stopWatchingDarkThemeMedia !== 'function')
            window.stopWatchingDarkThemeMedia = stop;

        if (typeof mtp.watchDarkThemeMedia !== 'function')
            mtp.watchDarkThemeMedia = watch;

        if (typeof mtp.stopWatchingDarkThemeMedia !== 'function')
            mtp.stopWatchingDarkThemeMedia = stop;
    }

    bridge();
})();

window.ff.download = function (fileName, base64) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:application/octet-stream;base64,' + base64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
