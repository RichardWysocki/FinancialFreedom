/**
 * MudBlazor 9.4.x bug: observeMainContainer() uses undefined identifier `containerClass` in a template
 * literal when the popover provider node is not found yet, causing ReferenceError and breaking dialogs.
 * Also retry for a few frames when .mud-popover-provider appears after first paint (WASM race).
 */
(function () {
    var mp = window.mudPopover;
    if (!mp || !mp.constructor || !mp.constructor.prototype) return;

    var proto = mp.constructor.prototype;
    var orig = proto.observeMainContainer;
    if (typeof orig !== "function") return;

    var maxRetries = 24;

    proto.observeMainContainer = function () {
        var helper = window.mudpopoverHelper;
        var cls = helper && helper.mainContainerClass;
        if (!cls) {
            return orig.call(this);
        }

        var el = document.body.getElementsByClassName(cls)[0];
        if (!el) {
            var self = this;
            var n = self._ffPopoverRetry | 0;
            if (n < maxRetries) {
                self._ffPopoverRetry = n + 1;
                requestAnimationFrame(function () {
                    self.observeMainContainer();
                });
                return;
            }
            self._ffPopoverRetry = 0;
            console.warn(
                "[FinancialFreedom] MudBlazor popover container not found for class \"" +
                    cls +
                    "\" after " +
                    maxRetries +
                    " frames. Check that <MudPopoverProvider /> is in App.razor."
            );
            return;
        }

        this._ffPopoverRetry = 0;
        return orig.call(this);
    };
})();
