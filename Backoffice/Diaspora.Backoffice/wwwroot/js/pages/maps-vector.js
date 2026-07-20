/**
 * Template Name: UBold - Admin & Dashboard Template
 * By (Author): Coderthemes
 * Module/App (File Name): Map Vector
 */

class VectorMap {

    init() {
        this.initWorldMapMarker();
        this.initWorldMarkerLine();
    }

    initVectorMap(selector, options = {}) {
        let element = null

        if (selector instanceof Element) {
            element = selector;
        } else {
            element = document.querySelector(selector);
        }

        if (element) {
            const map = new jsVectorMap({
                selector: element,
                ...options
            });

            window.addEventListener('resize', debounce(() => {
                map.updateSize();
            }, 200));
        }
    }

    initWorldMapMarker() {
        this.initVectorMap('#world-map-markers', {
            map: 'world',
            zoomOnScroll: false,
            zoomButtons: true,
            markersSelectable: true,
            markers: [
                {name: "Greenland", coords: [72, -42]},
                {name: "Canada", coords: [56.1304, -106.3468]},
                {name: "Brazil", coords: [-14.2350, -51.9253]},
                {name: "Egypt", coords: [26.8206, 30.8025]},
                {name: "Russia", coords: [61, 105]},
                {name: "China", coords: [35.8617, 104.1954]},
                {name: "United States", coords: [37.0902, -95.7129]},
                {name: "Norway", coords: [60.472024, 8.468946]},
                {name: "Ukraine", coords: [48.379433, 31.16558]},
            ],
            markerStyle: {
                initial: {fill: ins("primary")},
                selected: {fill: ins("primary")}
            },
            regionStyle: {
                initial: {
                    stroke: "#aab9d14d",
                    strokeWidth: 0.25,
                    fill: '#aab9d14d',
                    fillOpacity: 1,
                },
            },
            labels: {
                markers: {
                    render: marker => marker.name
                }
            }
        })
    }

    initWorldMarkerLine() {
        this.initVectorMap('#world-map-markers-line', {
            map: "world_merc",
            zoomOnScroll: false,
            zoomButtons: false,
            markers: [
                {
                    name: "Greenland",
                    coords: [72, -42]
                },
                {
                    name: "Canada",
                    coords: [56.1304, -106.3468]
                },
                {
                    name: "Brazil",
                    coords: [-14.2350, -51.9253]
                },
                {
                    name: "Egypt",
                    coords: [26.8206, 30.8025]
                },
                {
                    name: "Russia",
                    coords: [61, 105]
                },
                {
                    name: "China",
                    coords: [35.8617, 104.1954]
                },
                {
                    name: "United States",
                    coords: [37.0902, -95.7129]
                },
                {
                    name: "Norway",
                    coords: [60.472024, 8.468946]
                },
                {
                    name: "Ukraine",
                    coords: [48.379433, 31.16558]
                },
            ],
            lines: [{
                from: "Canada",
                to: "Egypt"
            },
                {
                    from: "Russia",
                    to: "Egypt"
                },
                {
                    from: "Greenland",
                    to: "Egypt"
                },
                {
                    from: "Brazil",
                    to: "Egypt"
                },
                {
                    from: "United States",
                    to: "Egypt"
                },
                {
                    from: "China",
                    to: "Egypt"
                },
                {
                    from: "Norway",
                    to: "Egypt"
                },
                {
                    from: "Ukraine",
                    to: "Egypt"
                },
            ],
            regionStyle: {
                initial: {
                    stroke: "#aab9d14d",
                    strokeWidth: 0.25,
                    fill: '#aab9d14d',
                    fillOpacity: 1,
                },
            },
            markerStyle: {
                initial: {fill: ins("secondary")},
                selected: {fill: ins("secondary")}
            },
            lineStyle: {
                animation: true,
                strokeDasharray: "6 3 6",
            },
        })
    }
}

window.loadMapsVector = function () {
    new VectorMap().init();
};