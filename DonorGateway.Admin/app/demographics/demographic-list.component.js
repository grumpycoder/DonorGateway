//demographic-list.component.js
(function () {
    var module = angular.module('app');

    function controller($http, log) {
        var $ctrl = this;
        var tableStateRef;
        var pageSizeDefault = 10;

        $ctrl.title = 'Demographic Updates';
        $ctrl.description = 'Updates made to constituent data';
        $ctrl.subTitle = 'Demographics';

        $ctrl.searchModel = {
            page: 1,
            pageSize: pageSizeDefault
        };

        $ctrl.sources = [
            { id: null, name: '' },
            { id: 0, name: 'Tax' },
            { id: 1, name: 'RSVP' }
        ];

        $ctrl.$onInit = function () { console.log('event detail init'); }

        $ctrl.search = function (tableState) {
            tableStateRef = tableState;
            $ctrl.isBusy = true;
            console.log('search', $ctrl.searchModel);

            if (typeof (tableState.sort.predicate) !== "undefined") {
                $ctrl.searchModel.orderBy = tableState.sort.predicate;
                $ctrl.searchModel.orderDirection = tableState.sort.reverse ? 'desc' : 'asc';
            }

            return $http.get('api/demographic', { params: $ctrl.searchModel })
                .then(function (r) {
                    $ctrl.demographics = r.data.results;
                    $ctrl.searchModel = r.data;
                    delete $ctrl.searchModel.results;
                }).catch(function (err) {
                    console.log('Oops', err.data.message);
                    toastr.error('Oops ' + err.data.message);
                }).finally(function () {
                    $ctrl.isBusy = false;
                });
        }

        $ctrl.delete = function (d) {
            $ctrl.isBusy = true;
            console.log('d', d);
            $http.delete('api/demographic/' + d.id).then(function (r) {
                var idx = $ctrl.demographics.indexOf(d);
                $ctrl.demographics.splice(idx, 1);
                log.success('Record updated');
            }).catch(function (err) {
                console.log('Oops', err);
                console.log(err);
                log.error('Oops. Something went wrong. ' + err.data.message);
            }).finally(function () {
                $ctrl.isBusy = false;
            });
        }

        $ctrl.deleteAll = function () {
            $ctrl.isBusy = true;
            return $http.delete('api/demographic').then(function (r) {
                log.warning('Updated all records');
                $ctrl.searchModel(tableStateRef);
            }).catch(function (err) {
                console.log('Oops', err);
                log.error('Oops. Something went wrong. ' + err.data.message);
            }).finally(function () {
                $ctrl.isBusy = false;
            });

        }

        $ctrl.export = function () {
            $ctrl.isBusy = true;
            $http.get('api/demographic/export', { params: $ctrl.searchModel })
                .then(function (data) {
                    var contentType = data.headers()['content-type'];
                    var filename = data.headers()['x-filename'];

                    var linkElement = document.createElement('a');
                    try {
                        var blob = new Blob([data.data], { type: contentType });
                        var url = window.URL.createObjectURL(blob);

                        linkElement.setAttribute('href', url);
                        linkElement.setAttribute("download", filename);

                        var clickEvent = new MouseEvent("click", {
                            "view": window,
                            "bubbles": true,
                            "cancelable": false
                        });
                        linkElement.dispatchEvent(clickEvent);
                    } catch (ex) {
                        logger.log(ex);
                    }
                }).finally(function () {
                    $ctrl.isBusy = false;
                });
        }


        $ctrl.paged = function paged() {
            $ctrl.search(tableStateRef);
        };

    }

    module.component('demographicList',
        {
            bindings: {
            },
            templateUrl: 'app/demographics/demographic-list.component.html',
            controller: ['$http', 'toastr', controller]
        });
}
)();