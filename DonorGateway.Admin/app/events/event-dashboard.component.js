//event-dashboard.component.js
(function () {
    var module = angular.module('app');

    function controller($http, toastr) {
        var $ctrl = this;

        $ctrl.title = 'Event Manager';
        $ctrl.description = "Manage Donor Events";

        $ctrl.$onInit = function () {
            console.log('event dashboard init');
            //$ctrl.isBusy = true;

            $http.get('api/event').then(function (r) {
                $ctrl.events = r.data;
            }).catch(function (err) {
                toastr.error("Oops. Can't get list of events");
            }).finally(function () {
                $ctrl.isBusy = false;
            });
        }

        $ctrl.changeEvent = function() {
            //console.log('selected event', $ctrl.selectedEvent);
        }
    }

    module.component('eventDashboard',
        {
            bindings: {
            },
            templateUrl: 'app/events/event-dashboard.component.html',
            controller: ['$http', 'toastr', controller]
        });
}
)();