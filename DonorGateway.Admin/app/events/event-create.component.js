//event-create.component.js
(function () {
    var module = angular.module('app');

    function eventCreateController($http, log) {
        var ctrl = this;

        ctrl.title = 'Create Event';
        ctrl.dateFormat = "MM/DD/YYYY h:mmA";

        ctrl.$onInit = function () {
            ctrl.event = {
                startDate: moment(new Date()).add(1, 'hour').startOf('hour'),
                endDate: moment(new Date()).add(5, 'days').startOf('hour'),
                capacity: 1,
                template: {}
            };
        }

        ctrl.cancel = function () {
            ctrl.dismiss();
        }

        ctrl.save = function () {
            ctrl.event.startDate = convertDate(ctrl.event.startDate);
            ctrl.event.endDate = convertDate(ctrl.event.endDate);

            $http.post('api/event', ctrl.event).then(function (r) {
                angular.extend(ctrl.event, r.data);
                console.log('event', ctrl.event);
                ctrl.modalInstance.close(ctrl.event);
            }).catch(function (err) {
                console.log('Oops. Something went wrong saving event', err);
                log.error('Oops. Something went wrong saving event');
            });
        }

        function convertDate(date) {
            if (date) return moment(date).format('YYYY-MM-DDTHH:mm');
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