// Infinite scroll for the ATS notifications page.
//
// Watches a sentinel element at the bottom of the feed and calls back into .NET when it
// scrolls into view, so the next cursor page loads without the reader pressing anything.
// The page still renders a real "Load more" button inside the sentinel - that is the
// keyboard path and the fallback if this script never runs.
//
// Follows the same shape as scrollSpy.js: a global object rather than an ES module, so it
// is loaded by a plain <script> tag in index.html alongside the others.
window.notificationScroll = {
    // Returns a handle the caller disposes. One observer per handle, so a page that
    // reloads its list can tear down the old one without touching any other.
    observe: function (sentinel, dotNetRef) {
        let observer = null;

        if (sentinel && typeof IntersectionObserver !== 'undefined') {
            observer = new IntersectionObserver(
                entries => {
                    for (const entry of entries) {
                        if (!entry.isIntersecting) {
                            continue;
                        }

                        // Fire and forget: .NET guards against overlapping loads itself,
                        // and awaiting here would hold the observer callback open.
                        dotNetRef.invokeMethodAsync('OnSentinelVisibleAsync');
                    }
                },
                {
                    // Start loading a little before the sentinel is actually on screen, so
                    // the next page is usually there by the time the reader reaches it.
                    rootMargin: '200px 0px'
                });

            observer.observe(sentinel);
        }

        return {
            dispose: function () {
                if (observer) {
                    observer.disconnect();
                    observer = null;
                }
            }
        };
    }
};
