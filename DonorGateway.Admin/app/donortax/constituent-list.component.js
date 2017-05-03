//tax-list.component.js
(function () {
    var module = angular.module('app');

    function controller($http, toastr, $modal) {
        var $ctrl = this;
        var tableStateRef;
        var pageSizeDefault = 10;

        $ctrl.title = 'Donor Tax Manager';
        $ctrl.subTitle = 'Constituents';

        $ctrl.searchModel = {
            page: 1,
            pageSize: pageSizeDefault,
            orderBy: 'id',
            orderDirection: 'asc'
        };

        $ctrl.$onInit = function () {
            console.log('tax list init');
        }

        $ctrl.search = function (tableState) {
            tableStateRef = tableState;

            if (typeof (tableState.sort.predicate) !== "undefined") {
                $ctrl.searchModel.orderBy = tableState.sort.predicate;
                $ctrl.searchModel.orderDirection = tableState.sort.reverse ? 'desc' : 'asc';
            }

            $ctrl.isBusy = true;
            console.log('model', $ctrl.searchModel);
            $http.get('api/constituent', { params: $ctrl.searchModel })
                .then(function (r) {
                    $ctrl.people = r.data.results;
                    $ctrl.searchModel = r.data;
                    delete $ctrl.searchModel.results;
                    $ctrl.isBusy = false;
                });
        }

        $ctrl.paged = function paged(pageNum) {
            $ctrl.search(tableStateRef);
        };

        $ctrl.edit = function (person) {
            $modal.open({
                component: 'constituentEdit',
                bindings: {
                    modalInstance: "<"
                },
                resolve: {
                    person: person
                },
                size: 'lg'
            }).result.then(function (result) {
                angular.extend(person, result);
                toastr.info('Saved ' + result.name);
            }, function (reason) {
            });
        }

        $ctrl.viewTaxes = function (person) {
            console.log('view tax', person);
            $modal.open({
                component: 'taxDetail',
                bindings: {
                    modalInstance: "<"
                },
                resolve: {
                    person: person
                },
                size: 'md'
            }).result.then(function (result) {
                angular.extend(item, result);
                toastr.info('Saved ' + result.title);
            }, function (reason) {
            });
        }
    }

    module.component('constituentList',
        {
            templateUrl: 'app/donortax/constituent-list.component.html',
            controller: ['$http', 'toastr', '$uibModal', controller]
        });
}
)();