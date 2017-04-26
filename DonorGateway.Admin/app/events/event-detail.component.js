//event-detail.component.js
(function () {
    var module = angular.module('app');

    function controller($http) {
        var $ctrl = this;

        $ctrl.$onChanges = function () {
            $ctrl.refresh();
        }

        $ctrl.$onInit = function () {
            console.log('event detail init', $ctrl);
        }

        $ctrl.refresh = function () {
            if ($ctrl.eventId === undefined) return;
            $ctrl.isBusy = true; 
            $http.get('api/event/' + $ctrl.eventId).then(function(r) {
                $ctrl.event = r.data;
            }).catch(function(err) {
                console.log('Opps. Something went wrong', err);
            }).finally(function() {
                $ctrl.isBusy = false; 
            }); 
        }
    }

    module.component('eventDetail',
        {
            bindings: {
                eventId: '<'
            },
            templateUrl: 'app/events/event-detail.component.html',
            controller: ['$http', 'toastr', controller]
        });
}
)();