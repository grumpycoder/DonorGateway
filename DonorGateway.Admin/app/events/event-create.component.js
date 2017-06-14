//event-create.component.js
(function () {
    var module = angular.module('app');

    function eventCreateController($http, log) {
        var ctrl = this;

        ctrl.title = 'Create Event';
        ctrl.dateFormat = "MM/DD/YYYY hh:mm";

        ctrl.$onInit = function () {
            ctrl.event = {
                startDate: new Date(),
                capacity: 1,
                template: {}
            };
        }

        ctrl.cancel = function () {
            $modal.dismiss();
        }

        ctrl.save = function () {
            $http.post('api/event', ctrl.event).then(function (r) {
                angular.extend(ctrl.event, r.data);
                console.log('event', ctrl.event);
                ctrl.modalInstance.close(ctrl.event);
            }).catch(function (err) {
                console.log('Oops. Something went wrong saving event', err);
                log.error('Oops. Something went wrong saving event');
            });
        }

    }

    module.component('eventCreate',
        {
            bindings: {
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            templateUrl: 'app/events/event-create.component.html',
            controller: ['$http', 'toastr', eventCreateController]
        });

}
)();