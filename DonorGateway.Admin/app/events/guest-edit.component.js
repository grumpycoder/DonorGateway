//guest-edit.component.js
(function () {
    var module = angular.module('app');

    function guestEditController($http, log) {
        var ctrl = this;
        ctrl.ticketCountList = [];

        ctrl.$onInit = function () {
            console.log('guest edit init');

            if (ctrl.resolve) {
                ctrl.id = ctrl.resolve.guestId;
                ctrl.eventId = ctrl.resolve.eventId;
            }

            getGuestDetails().then(function (r) {
                ctrl.guest = r;
            }).then(function () {
                ctrl.title = ctrl.guest.name || 'New Guest'; 
                for (var i = 1; i < (ctrl.guest.ticketAllowance || 5) + 1; i++) {
                    ctrl.ticketCountList.push(i);
                }
            });
        }

        function getGuestDetails() {
            if (ctrl.id) {
                return $http.get('api/event/guest/' + ctrl.id).then(function (r) {
                    return r.data;
                }).catch(function (err) {
                    console.log('err', err.message);
                    log.error('Oops. Something went wrong getting guest information');
                });
            } else {
                return $http.get('api/event/' + ctrl.eventId).then(function (r) {
                    return { eventId: ctrl.eventId, ticketAllowance: r.data.ticketAllowance }
                }).catch(function (err) {
                    console.log('Opps. Something went wrong getting event', err);
                });
            }
        }

        ctrl.changeAttending = function () {
            if (ctrl.guest.isAttending) {
                ctrl.guest.ticketCount = 1;
            } else {
                ctrl.guest.ticketCount = null;
            }
        }

        ctrl.changeTicket = function () {
            ctrl.guest.isAttending = ctrl.guest.ticketCount !== null;
        }

        ctrl.cancel = function () {
            ctrl.dismiss();
        }

        ctrl.save = function () {
            $http.post('api/event/' + ctrl.guest.eventId + '/register/', ctrl.guest).then(function (r) {
                angular.extend(ctrl.guest, r.data);
                ctrl.modalInstance.close(ctrl.guest);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                log.error('Oops. Something went wrong registering guest', err.data.message);
            });
        }
    }

    module.component('guestEdit',
        {
            bindings: {
                guestId: '<',
                eventId: '<',
                resolve: '<',
                close: '&',
                dismiss: '&',
                modalInstance: '<'
            },
            templateUrl: 'app/events/guest-edit.component.html',
            controller: ['$http', 'toastr', guestEditController]
        });

}
)();