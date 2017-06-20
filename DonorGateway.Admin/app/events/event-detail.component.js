//event-detail.component.js
(function () {
    var module = angular.module('app');

    function parseErrors(response) {
        var errors = [];
        var key;
        for (key in response.modelState) {
            if (response.modelState.hasOwnProperty(key)) {
                for (var i = 0; i < response.modelState[key].length; i++) {
                    if (key === '$id') break;
                    errors.push(response.modelState[key][i]);
                }
            }
        }
        return errors;
    }

    function eventDetailController($http, log, $scope) {
        var ctrl = this;

        ctrl.dateFormat = "MM/DD/YYYY h:mmA";
        ctrl.hostLocation = window.__env.rsvpUrl + '/';

        ctrl.$onChanges = function () {
            ctrl.refreshEvent();
        }

        ctrl.$onInit = function () { console.log('event detail init'); }

        ctrl.refreshEvent = function () {
            if (ctrl.eventId === undefined) return;
            ctrl.isBusy = true;
            $http.get('api/event/' + ctrl.eventId).then(function (r) {
                ctrl.event = r.data;
                ctrl.nameUrl = r.data.nameUrl;
            }).catch(function (err) {
                console.log('Opps. Something went wrong', err);
            }).finally(function () {
                ctrl.isBusy = false;
            });
        }

        ctrl.delete = function () {
            $http.delete('api/event/' + ctrl.eventId).then(function (r) {
                ctrl.event = null;
                log.warning('Deleted event');
                ctrl.onDelete();
            }).catch(function (err) {
                console.log('Oops. Something went wrong deleting event', err);
                log.error('Oops. Something went wrong deleting event');
            });

        }

        ctrl.saveEvent = function () {
            ctrl.isBusy = true;
            return $http.put('api/event', ctrl.event)
                .then(function (r) {
                    angular.extend(ctrl.event, r.data);
                    log.success('Updated ' + ctrl.event.initiative);
                    ctrl.onUpdated({ event: ctrl.event });
                }).catch(function (err) {
                    console.log('Oops. Something went wrong saving event');
                    log.error('Oops. Something went wrong saving event');
                    ctrl.errors = parseErrors(err.data);
                }).finally(function () {
                    ctrl.isBusy = false;
                });
        }

        ctrl.toggleCancel = function () {
            ctrl.event.isCancelled = !ctrl.event.isCancelled;
            ctrl.saveEvent();
        }

        function convertDate(date) {
            if (date) return moment(date).format('YYYY-MM-DDTHH:mm');
        }
    }

    module.component('eventDetail',
        {
            bindings: {
                onDelete: '&',
                eventId: '<',
                onUpdated: '&'
            },
            templateUrl: 'app/events/event-detail.component.html',
            controller: ['$http', 'toastr', '$scope', eventDetailController]
        });
}
)();