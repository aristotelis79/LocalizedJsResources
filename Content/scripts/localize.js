var resources;
var resourcesdb;

function t(key) {

    if ($.isEmptyObject(resources)) {
        Localize.loadResources();
    }
    
    if (key in resources) {
        return resources[key];
    }
    return key;
}


var Localize = (function () {

    if (!resourcesdb) {
        resourcesdb = localforage.createInstance({
            name: 'nop-localized-javascript'
        });
    }

    $('#customerlanguage').on('change', function () {
        resourcesdb.setItem('isReasourcesLoad', false);
    });


    resourcesdb.ready().then(function() {
        resourcesdb.getItem('isReasourcesLoad')
            .then(function(value) {
                if (!value) {
                    loadResources();
                } else {
                    resourcesdb.getItem('t')
                        .then(function(values) {
                            if (!values) {
                                loadResources();
                            } else {
                                resources = values;
                            }
                        });
                }
            });
    });

    function loadResources() {
        var languageId = findLanguageId();
        if (languageId) {
            getResourceValues(languageId);
        }
    }

    function findLanguageId() {
        var customerlanguage = $('#customerlanguage').find(":selected").val();
        var lanhguageIdPatern = customerlanguage.match(/[\/][1-9]+[?]/gi);
        if (lanhguageIdPatern) {
            return lanhguageIdPatern[0].replace('/', '').replace('?', '');
        }
    }

    function getResourceValues(languageId) {
        $.ajax({
            cached: false,
            type: "get",
            url: "LocalizedJavascript/GetAllResourceValues",
            data: { languageId: languageId },
            success: function (json) {
                resourcesdb.setItem('t', json).then(function (json) {
                    resources = json;
                    resourcesdb.setItem('isReasourcesLoad', true);
                }).catch(function (err) {
                    console.log(err);
                });
            },
            error: function (err) {
                console.log(err);
            },
            complete: function (data) { }
        });
    }

    return {
        loadResources:loadResources
    }

    //if (typeof (resourcesdb) == undefined) {
    //    import * as resourcesdb from "/Plugins/Progressive.Web.App/Content/Scripts/node_modules/localforage/dist/localforage.min.js";
    //}

    //$.getScript("/Plugins/Progressive.Web.App/Content/Scripts/node_modules/localforage/dist/localforage.min.js", function() {
    //    alert("Script loaded but not necessarily executed.");
    //});

    //function dynamicallyLoadScript(url) {
    //    var script = document.createElement("script"); // Make a script DOM node
    //    script.src = url; // Set it's src to the provided URL

    //    document.head.appendChild(script); // Add it to the end of the head section of the page (could change 'head' to 'body' to add it to the end of the body section instead)
    //}

    //function loadReasourcesFromFile() {
    //    var filepath = '/Plugins/Localized.Javascript/Content/locales/English/lang.json';

    //    var lang = $('#customerlanguage').find(':selected').text();
    //    if (lang) {
    //        filepath = '/Plugins/Localized.Javascript/Content/locales/' + lang + '/lang.json';
    //    }

    //    readResourcesFromFile(filepath);
    //};

    //function readResourcesFromFile(filepath) {
    //    $.getJSON(filepath, function (json) {
    //        resourcesdb.setItem('t', json).then(function (json) {
    //            t = json;
    //            resourcesdb.setItem('isReasourcesLoad', true);
    //        }).catch(function (err) {
    //            console.log(err);
    //        });
    //    });
    //}

})();