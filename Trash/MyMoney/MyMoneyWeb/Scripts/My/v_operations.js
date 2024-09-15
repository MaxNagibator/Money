// todo остановится и прибрать говно уже, убрать дублирование, даты ужос О_О
var currentOperationMode;
function SetMode(mode, shift) {
    if (!shift) {
        shift = 0;
    }
    var dateFrom = $('#dateFrom').val();
    var dateTo = $('#dateFrom').val();
    var date = dateFrom.split('.');
    var d = new Date(date[2], date[1] - 1, date[0]);
    var currMonth = d.getMonth();
    var currYear = d.getFullYear();
    var currDay = d.getDate();
    var currDayOfWeek = d.getDay();
    if (currDayOfWeek == 0) {
        currDayOfWeek = 7; // еврейское какоето начало недели
    }

    if (mode == 'day') {
        dateFrom = d;
        if (shift != 0) {
            dateFrom.setDate(dateFrom.getDate() + shift);
        }
        dateTo = new Date(d);
        dateTo.setDate(dateTo.getDate() +1);
    }
    if (mode == 'week') {
        dateFrom = d;
        dateFrom.setDate(dateFrom.getDate() - ((currDayOfWeek - 1) % 7));
        if (shift != 0) {
            dateFrom.setDate(dateFrom.getDate() + (shift * 7));
        }
        dateTo = new Date(d);
        dateTo.setDate(dateTo.getDate() + 7);
    }
    if (mode == 'month') {
        dateFrom = d;
        dateFrom.setDate(1);
        if (shift != 0) {
            dateFrom.setMonth(dateFrom.getMonth() + shift);
        }
        dateTo = new Date(dateFrom);
        dateTo.setMonth(dateTo.getMonth() + 1);
    }
    if (mode == 'year') {
        dateFrom = d;
        dateFrom.setDate(1);
        dateFrom.setMonth(0);
        if (shift != 0) {
            dateFrom.setYear(dateFrom.getFullYear() + shift);
        }
        dateTo = new Date(dateFrom);
        dateTo.setYear(dateTo.getFullYear() + 1);
    }

    dateTo.setDate(dateTo.getDate() - 1);

    $('#dateFrom').data('DateTimePicker').setDate(dateFrom);
    $('#dateTo').data('DateTimePicker').setDate(dateTo);
    currentOperationMode = mode;
    $('.mode-btn').removeClass('mode-active');
    $('.mode-' + currentOperationMode).addClass('mode-active');
    GetPayments();
}

function GetMyDate(d) {
    var currDay1 = d.getDate();
    var currMonth1 = d.getMonth() + 1;
    var currYear1 = d.getFullYear();
    var month = currMonth1 < 10 ? '0' + currMonth1 : currMonth1;
    var day = currDay1 < 10 ? '0' + currDay1 : currDay1;

    return day + '.' + month + '.' + currYear1;
}

function SetPeriod(shift) {
    SetMode(currentOperationMode, shift);
}

var operationTabClicked = 'operationsTab';

function GetPayments() {
    var filter = GetFilter();
    $.ajax({
        url: '/Finance/GetPayments',
        type: "POST",
        data: filter,
        traditional: true
    }).done(function(result) {
        $('#payments').html(result);
    }).fail(function(xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function() {
        $('#' + operationTabClicked).click();
    });
}

function GetFilter() {
    var dateTo = $('#dateTo').val();
    var date = dateTo.split('.');
    var d = new Date(date[2], date[1] - 1, date[0]);
    d.setDate(d.getDate() + 1);
    dateTo = GetMyDate(d);
    var f = {
        from: $('#dateFrom').val(),
        to: dateTo,
        categoryIds: DropDownPickerGetValue('filterCategoryListbox'),
        comment: $('#searchComment').val(),
        place: $('#searchPlace').val(),
    };
    return f;
}
function Excel() {
    var filter = GetFilter();
    $.ajax({
        url: '/Finance/GetPaymentsExcel',
        type: "POST",
        data: filter,
        traditional: true
    }).done(function (result) {
        if (result.success) {
            var filename = result.filename;
            var url = '/Finance/GetExcelFile?filename=' + filename;
            window.open(url, '_blank');
        } else {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 3000);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function ChangeCategory() {
    $('#changeCategoryConfirmBtn').show();
    $('#changeCategoryBtn').hide();
}

function ChangeCategoryConfirm() {
    var ids = [];
    var rows = $('.payment-row');
    for (var i = 0; i < rows.length; i++) {
        ids.push($(rows[i]).data('payment-id'));
    }
    
    $.ajax({
        url: '/Finance/UpdatePaymentsBatch',
        type: "POST",
        data: {
            ids: ids,
            categoryId: DropDownPickerGetValue('changeCategoryListbox'),
        },
        traditional: true
    }).done(function (result) {
        if (result.success) {
            $('#changeCategoryConfirmBtn').hide();
            $('#changeCategoryBtn').show();
            GetPayments();
        } else {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 3000);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function AddPayment(paymentTypeId, callback) {
    $.ajax({
        url: '/Finance/GetPaymentForm',
        type: "POST",
        data: { paymentTypeId: paymentTypeId },
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else {
            $('#addPaymentBtn').hide();
            $('#paymentFormBaseDiv').html(result);
            $('#paymentFormBaseDivPanel').removeClass('hidden');
            if (callback) {
                callback();
            }
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function AddPaymentByDayHeader(paymentTypeId, defaultDate, dayId, callback) {
    $.ajax({
        url: '/Finance/GetPaymentForm',
        type: "POST",
        data: { paymentTypeId: paymentTypeId, defaultDate: defaultDate, dayId: dayId },
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else {
            $('#' + dayId + ' #addPaymentBtn').hide();
            $('#' + dayId + ' #paymentFormDiv').html(result);
            if (callback) {
                callback();
            }
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

var prevPaymentFormPlaceSearchString = '';
var changePaymentFormPlaceTimerId = -1;
function PaymentFormPlaceKeyPress(focus) {
    var paymentFormPlaceSearchString = $('#paymentFormPlace').val();
    if (focus == 1 || paymentFormPlaceSearchString.length > 0 && prevPaymentFormPlaceSearchString.length == 0) {
        if (changePaymentFormPlaceTimerId != -1) {
            clearTimeout(changePaymentFormPlaceTimerId);
        }
        changePaymentFormPlaceTimerId = setTimeout(function () {
            ChangePaymentFormPlace(paymentFormPlaceSearchString);
        }, 500);
    }
}

function ChangePaymentFormPlace(searchString) {
    prevPaymentFormPlaceSearchString = searchString;
    $.ajax({
        url: '/Finance/GetListHelp',
        type: "POST",
        data: { key: 'place', searchString: searchString },
        success: function (result) {
            if (result.success == false) {
                console.log('ChangePaymentFormPlace -> get help words: ' + result.message);
            } else {
                $('#paymentFormPlaceListTemp').html(result);
                if ($('#paymentFormPlaceList').html() != $('#paymentFormPlaceListTemp').html()) {
                    $('#paymentFormPlaceList').html($('#paymentFormPlaceListTemp').html());
                }
            }
            prevPaymentFormPlaceSearchString = '';
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log('ChangePaymentFormPlace  -> get help words: troubles');
            prevPaymentFormPlaceSearchString = '';
        }
    });
}

var prevSearchPlaceString = '';
var changeSearchPlaceTimerId = -1;
function SearchPlaceKeyPress(focus) {
    var searchPlaceString = $('#searchPlace').val();
    if (focus == 1 || searchPlaceString.length > 0 && prevPaymentFormPlaceSearchString.length == 0) {
        if (changeSearchPlaceTimerId != -1) {
            clearTimeout(changeSearchPlaceTimerId);
        }
        changeSearchPlaceTimerId = setTimeout(function () {
            ChangeSearchPlace(searchPlaceString);
        }, 500);
    }
}

function ChangeSearchPlace(searchString) {
    prevSearchPlaceString = searchString;
    $.ajax({
        url: '/Finance/GetListHelp',
        type: "POST",
        data: { key: 'place', searchString: searchString },
        success: function (result) {
            if (result.success == false) {
                console.log('ChangeSearchPlace -> get help words: ' + result.message);
            } else {
                $('#searchPlaceListTemp').html(result);
                if ($('#searchPlaceList').html() != $('#searchPlaceListTemp').html()) {
                    $('#searchPlaceList').html($('#searchPlaceListTemp').html());
                }
            }
            prevSearchPlaceString = '';
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log('ChangeSearchPlace  -> get help words: troubles');
            prevSearchPlaceString = '';
        }
    });
}

function SavePayment(dayId) {
    var data;
    if (dayId != undefined && dayId != '') {
        data = $('#' + dayId + ' #paymentForm').serialize();
    } else {
        data = $('#paymentForm').serialize();
    }
    $.ajax({
        url: '/Finance/SavePayment',
        type: "POST",
        data: data,
        traditional: true
    }).done(function (result) {
        if (result.success == true) {
            if (dayId != undefined && dayId != '') {
                $('#' + dayId + ' #paymentFormDiv').empty();
                $('#' + dayId + ' #addPaymentBtn').show();
                UpdatePaymentDayInfo(result.paymentId, result.dayId);
            } else {
                UpdatePaymentDayInfo(result.paymentId, result.dayId);
                $('#paymentFormBaseDiv').empty();
                $('#paymentFormBaseDivPanel').addClass('hidden');
                $('#addPaymentBtn').show();
            }
        } else if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        }
        else {
            if (dayId != undefined && dayId != '') {
                $('#' + dayId + ' #paymentFormDiv').html(result);
            } else {
                $('#paymentFormBaseDiv').html(result);
                $('#paymentFormBaseDivPanel').removeClass('hidden');
            }
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function CancelPayment(dayId) {
    if (dayId != undefined && dayId != '') {
        $('#' + dayId + ' #paymentFormDiv').empty();
        $('#' + dayId + ' #addPaymentBtn').show();
    } else {
        $('#paymentFormBaseDiv').empty();
        $('#paymentFormBaseDivPanel').addClass('hidden');
        $('#addPaymentBtn').show();
    }
}

//function EditPayment(paymentId) {
//    $.ajax({
//        url: '/Finance/GetPaymentForm',
//        type: "POST",
//        data: { paymentId: paymentId },
//        traditional: true
//    }).done(function (result) {
//        if (result.success == false) {
//            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
//        } else {
//            $('#addPaymentBtn').hide();
//            $('#paymentFormBaseDiv').html(result);
//        }
//    }).fail(function (xhr, textStatus, errorThrown) {
//        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
//    }).always(function () {
//    });
//}

function DeletePayment(paymentId) {
    $('#payment_' + paymentId + ' .confirm-delete').removeClass('hidden');
    $('#payment_' + paymentId + ' .cancel-delete').removeClass('hidden');
    $('#payment_' + paymentId + ' .start-delete').addClass('hidden');
}

function CancelDeletePayment(paymentId) {
    $('#payment_' + paymentId + ' .confirm-delete').addClass('hidden');
    $('#payment_' + paymentId + ' .cancel-delete').addClass('hidden');
    $('#payment_' + paymentId + ' .start-delete').removeClass('hidden');
}
function ConfirmDeletePayment(paymentId) {
    $.ajax({
        url: '/Finance/DeletePayment',
        type: "POST",
        data: { paymentId: paymentId },
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else {
            var typeId = $('#payment_' + paymentId + ' .show-sum').closest('.payment-row').data('payment-type-id');
            var prevSum = toIn($('#payment_' + paymentId + ' .show-sum').html());
            var currentDayId = $('#payment_' + paymentId + ' .show-sum').closest('.payment-day').attr('id');
            var prevDaySum = toIn($('#' + currentDayId + ' .payment-day-title-sum-' + typeId).html());
            $('#' + currentDayId + ' .payment-day-title-sum-' + typeId).html(toOut(prevDaySum - prevSum));
            $('#payment_' + paymentId).remove();
            RefreshStats();
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function EditPayment2(paymentId) {
    $('#payment_' + paymentId + ' .confirm-edit').removeClass('hidden');
    $('#payment_' + paymentId + ' .cancel-edit').removeClass('hidden');
    $('#payment_' + paymentId + ' .start-edit').addClass('hidden');

    $('#payment_' + paymentId + ' .edit-sum').removeClass('hidden');
    $('#payment_' + paymentId + ' .show-sum').addClass('hidden');
    $('#payment_' + paymentId + ' .edit-comment').removeClass('hidden');
    $('#payment_' + paymentId + ' .show-comment').addClass('hidden');
    $('#payment_' + paymentId + ' .edit-place').removeClass('hidden');
    $('#payment_' + paymentId + ' .show-place').addClass('hidden');
    $('#payment_' + paymentId + ' .edit-catname').removeClass('hidden');
    $('#payment_' + paymentId + ' .show-catname').addClass('hidden');
    $('#payment_' + paymentId + ' .edit-date').removeClass('hidden');
    $('#payment_' + paymentId + ' .show-date').addClass('hidden');

    if (!$('#payment_' + paymentId + ' .edit-catname').hasClass('init-select')) {
        $('#categoryforedit')
            .clone()
            .prop('id', 'payment_cat_' + paymentId)
            .data('payment-id', paymentId)
            .appendTo('#payment_' + paymentId + ' .edit-catname');
    }
    var id = $('#payment_' + paymentId + ' .edit-catname input').val();
    $('#payment_' + paymentId + ' .edit-catname').addClass('init-select');
    $('#payment_cat_' + paymentId).val(id);
}

function editCategoryChange(elem) {
    var paymentId = $(elem).data('payment-id');
    var val = $(elem).val();
    $('#payment_' + paymentId + ' .edit-catname input').val(val);
}

function CancelEditPayment2(paymentId) {
    $('#payment_' + paymentId + ' .confirm-edit').addClass('hidden');
    $('#payment_' + paymentId + ' .cancel-edit').addClass('hidden');
    $('#payment_' + paymentId + ' .start-edit').removeClass('hidden');

    // повешать обищий класс как нить
    $('#payment_' + paymentId + ' .edit-sum').addClass('hidden');
    $('#payment_' + paymentId + ' .show-sum').removeClass('hidden');
    $('#payment_' + paymentId + ' .edit-comment').addClass('hidden');
    $('#payment_' + paymentId + ' .show-comment').removeClass('hidden');
    $('#payment_' + paymentId + ' .edit-place').addClass('hidden');
    $('#payment_' + paymentId + ' .show-place').removeClass('hidden');
    $('#payment_' + paymentId + ' .edit-catname').addClass('hidden');
    $('#payment_' + paymentId + ' .show-catname').removeClass('hidden');
    $('#payment_' + paymentId + ' .edit-date').addClass('hidden');
    $('#payment_' + paymentId + ' .show-date').removeClass('hidden');
}

function ConfirmEditPayment2(paymentId) {
    var sum = $('#payment_' + paymentId + ' .edit-sum input').val();
    var comment = $('#payment_' + paymentId + ' .edit-comment textarea').val();
    var place = $('#payment_' + paymentId + ' .edit-place input').val();
    var categoryId = $('#payment_' + paymentId + ' .edit-catname input').val();
    var date = $('#payment_' + paymentId + ' .edit-date input').val();
    $.ajax({
        url: '/Finance/UpdatePayment',
        type: "POST",
        data: {
            id: paymentId,
            sum: sum,
            categoryId: categoryId,
            date: date,
            comment: comment,
            place: place
        },
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else if (result.success == true) {
            ShowAlert('Успех!', "Обновлено", window.AlertType.Success, 2000);
            CancelEditPayment2(paymentId);
            UpdatePaymentDayInfo2(paymentId, result.dayId, result.sum);
            //UpdatePaymentRow(paymentId);
        } else {
            ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function UpdatePaymentRow(paymentId) {
    var dayBlock = $('#payment_' + paymentId).closest('.payment-day');
}

function UpdatePaymentDayInfo(paymentId, dayId) {
    $.ajax({
        url: '/Finance/GetPaymentDay',
        type: "POST",
        data: { paymentId: paymentId },
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            SystemConsoleLog('UpdatePaymentDayInfo trouble:' + result.message, window.MessageType.Warning);
        } else {
            SystemConsoleLog('UpdatePaymentDayInfo good', window.MessageType.Success);
            if ($('#' + dayId).length > 0) {
                $('#' + dayId).replaceWith($.parseHTML(result));
            } else {
                $('#paymentList').prepend(result);
            }
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        SystemConsoleLog('UpdatePaymentDayInfo trouble', window.MessageType.Error);
    }).always(function () {
    });
    RefreshStats();
}

function toIn(val) {
    if (val != undefined) {
        return parseFloat(parseFloat(val.toString().replace(',', '.')).toFixed(2));
    }
}

function toOut(val) {
    if (val != undefined) {
        return val.toFixed(2).toString().replace('.', ',');
    }
}

function UpdatePaymentDayInfo2(paymentId, dayId, sumServer) {
    $('#payment_' + paymentId + ' .edit-sum input').val(sumServer);
    var sum = toIn(sumServer);
    var comment = $('#payment_' + paymentId + ' .edit-comment textarea').val();
    var place = $('#payment_' + paymentId + ' .edit-place input').val();
    var categoryId = $('#payment_' + paymentId + ' .edit-catname input').val();
    var categoryName = $('#payment_' + paymentId + ' .edit-catname select option:selected').text();
    var date = $('#payment_' + paymentId + ' .edit-date input').val();
    //var dayId = GetDateString(date);
    var currentDayId = $('#payment_' + paymentId + ' .show-sum').closest('.payment-day').attr('id');
    var typeId = $('#payment_' + paymentId + ' .show-sum').closest('.payment-row').data('payment-type-id');
    var prevSum = toIn($('#payment_' + paymentId + ' .show-sum').html());
    $('#payment_' + paymentId + ' .show-sum').html(toOut(sum));
    $('#payment_' + paymentId + ' .show-comment').html(comment);
    $('#payment_' + paymentId + ' .show-place').html(place);
    $('#payment_' + paymentId + ' .show-place').attr('title', place);
    $('#payment_' + paymentId + ' .show-catname').html(categoryName);
    $('#payment_' + paymentId + ' .show-catname').attr('title', categoryName);
    if (currentDayId == dayId) {
        var prevDaySum = toIn($('#' + dayId + ' .payment-day-title-sum-' + typeId).html());
        $('#' + dayId + ' .payment-day-title-sum-' + typeId).html(toOut(prevDaySum + sum - prevSum));
    } else {
        var prevDaySum = toIn($('#' + currentDayId + ' .payment-day-title-sum-' + typeId).html());
        $('#' + currentDayId + ' .payment-day-title-sum-' + typeId).html(toOut(prevDaySum - prevSum));

        var prevDaySum = toIn($('#' + dayId + ' .payment-day-title-sum-' + typeId).html());
        $('#' + dayId + ' .payment-day-title-sum-' + typeId).html(toOut(prevDaySum + sum));

        if ($('#' + dayId + ' .payment-day-body').length) {
            var row = $('#payment_' + paymentId);
            $('#' + dayId + ' .payment-day-body').append(row);
        } else {
            $('#payment_' + paymentId).remove();
        }
    }
    RefreshStats();
}

function GetDateString(date) {
    var ds = date.split('.');
    return 'day-' + ds[2] + '-' + ds[1] + '-' + ds[0];
    //var dt = new Date(date);
    //var yyyy = dt.getFullYear();
    //var mm = dt.getMonth()+1;
    //var dd = dt.getDay();

    //mm = mm < 10 ? '0' + mm : mm;
    //dd = dd < 10 ? '0' + dd : dd;
    //var str = 'day-' + '-' + mm + '-' + dd;
    //return str;
}

function RefreshStats() {
    $.ajax({
        url: '/Finance/GetPaymentsStats',
        type: "POST",
        data: {
            from: $('#dateFrom').val(),
            to: $('#dateTo').val(),
            categoryIds: DropDownPickerGetValue('filterCategoryListbox'),
            comment: $('#searchComment').val(),
            place: $('#searchPlace').val(),
        },
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            SystemConsoleLog('UpdateStats trouble:' + result.message, window.MessageType.Warning);
        } else {
            SystemConsoleLog('UpdateStats good', window.MessageType.Success);
            $('#stats').html(result);
            var totalSum1 = $('#TotalSumValue-1').data('total-sum');
            if (totalSum1 == null || totalSum1 == undefined) {
                totalSum1 = 0;
            }
            $('#TotalSumHeaderText-1').text(totalSum1);
            var totalSum2 = $('#TotalSumValue-2').data('total-sum');
            if (totalSum2 == null || totalSum2 == undefined) {
                totalSum2 = 0;
            }
            $('#TotalSumHeaderText-2').text(totalSum2);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        SystemConsoleLog('UpdateStats trouble', window.MessageType.Error);
    }).always(function () {
    });
}

function UpdateStatistics() {

}


function GetRegularTasksIfEmpty() {
    if ($('#regularTasks').hasClass('init')) {
        return;
    }
    GetRegularTasks();
}
function GetRegularTasks() {
    $.ajax({
        url: '/Finance/GetRegularTasks',
        type: "POST",
        data: {},
        traditional: true
    }).done(function (result) {
        $('#regularTasks').html(result);
        $('#regularTasks').addClass('init');
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function AddRegularTask() {
    $.ajax({
        url: '/Finance/GetRegularTaskForm',
        type: "POST",
        data: {},
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else {
            $('#regularTaskModal .modal-title').html('Создание регулярной задачи');
            $('#regularTaskModal .modal-footer .btn-confirm').html('Создать');
            $('#regularTaskModal .modal-footer .btn-cancel').html('Отмена создания');
            $('#regularTaskModal .modal-body').html(result);
            $('#regularTaskModal').modal();
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function EditRegularTask(elem) {
    var regularTaskId = $(elem).closest('.regular-task-item').data('regular-task-id');
    $.ajax({
        url: '/Finance/GetRegularTaskForm',
        type: "POST",
        data: { regularTaskId: regularTaskId},
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else {
            $('#regularTaskModal .modal-title').html('Редактирование регулярной задачи');
            $('#regularTaskModal .modal-footer .btn-confirm').html('Сохранить');
            $('#regularTaskModal .modal-footer .btn-cancel').html('Отмена редактирования');
            $('#regularTaskModal .modal-body').html(result);
            $('#regularTaskModal').modal();
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function SaveRegularTask() {
    var data = $('#regularTaskForm').serialize();
    $.ajax({
        url: '/Finance/SaveRegularTask',
        type: "POST",
        data: data,
        traditional: true
    }).done(function (result) {
        if (result.success == true) {
            $('#regularTaskModal').modal('hide');
            GetRegularTasks();
        } else if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        }
        else {
            $('#regularTaskModal .modal-body').html(result);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function DeleteRegularTask(elem) {
    var regularTaskId = $(elem).closest('.regular-task-item').data('regular-task-id');
    $.ajax({
        url: '/Finance/DeleteRegularTask',
        type: "POST",
        data: { regularTaskId: regularTaskId},
        traditional: true
    }).done(function (result) {
        if (result.success == true) {
            $(elem).closest('.regular-task-item').remove();
        } else if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function GetFastOperationsIfEmpty() {
    if ($('#fastOperations').hasClass('init')) {
        return;
    }
    GetFastOperations();
}
function GetFastOperations() {
    $.ajax({
        url: '/FinanceFastOperations/GetFastOperations',
        type: "POST",
        data: {},
        traditional: true
    }).done(function (result) {
        $('#fastOperations').html(result);
        $('#fastOperations').addClass('init');
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function AddFastOperation() {
    $.ajax({
        url: '/FinanceFastOperations/GetFastOperationForm',
        type: "POST",
        data: {},
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else {
            $('#fastOperationModal .modal-title').html('Создание быстрой операции');
            $('#fastOperationModal .modal-footer .btn-confirm').html('Создать');
            $('#fastOperationModal .modal-footer .btn-cancel').html('Отмена создания');
            $('#fastOperationModal .modal-body').html(result);
            $('#fastOperationModal').modal();
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function EditFastOperation(elem) {
    var fastOperationId = $(elem).closest('.fast-operation-item').data('fast-operation-id');
    $.ajax({
        url: '/FinanceFastOperations/GetFastOperationForm',
        type: "POST",
        data: { fastOperationId: fastOperationId },
        traditional: true
    }).done(function (result) {
        if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        } else {
            $('#fastOperationModal .modal-title').html('Редактирование быструю операцию');
            $('#fastOperationModal .modal-footer .btn-confirm').html('Сохранить');
            $('#fastOperationModal .modal-footer .btn-cancel').html('Отмена редактирования');
            $('#fastOperationModal .modal-body').html(result);
            $('#fastOperationModal').modal();
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function SaveFastOperation() {
    var data = $('#fastOperationForm').serialize();
    $.ajax({
        url: '/FinanceFastOperations/SaveFastOperation',
        type: "POST",
        data: data,
        traditional: true
    }).done(function (result) {
        if (result.success == true) {
            $('#fastOperationModal').modal('hide');
            GetFastOperations();
        } else if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        }
        else {
            $('#fastOperationModal .modal-body').html(result);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}

function DeleteFastOperation(elem) {
    var FastOperationId = $(elem).closest('.fast-operation-item').data('fast-operation-id');
    $.ajax({
        url: '/FinanceFastOperations/DeleteFastOperation',
        type: "POST",
        data: { FastOperationId: FastOperationId },
        traditional: true
    }).done(function (result) {
        if (result.success == true) {
            $(elem).closest('.fast-operation-item').remove();
        } else if (result.success == false) {
            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 2000);
        }
    }).fail(function (xhr, textStatus, errorThrown) {
        ShowAlert('Внимание!', commonErrorMessage, window.AlertType.Error, 2000);
    }).always(function () {
    });
}


function GetFromFastOperation(el, dayId) {
    var menu = $('#paymentFastOperationTemplate').clone();
    $(el).replaceWith(menu);
    setTimeout(function () {
        // а чёто сходу не понял как открыть, и так закостылил
        $('#' + dayId + ' #paymentFastOperationTemplate i').click();
    }, 50);

    $('#' + dayId + ' #paymentFastOperationTemplate .fast-operation').click(function (e) {
        var elem = e.target;
        var paymentType = $(elem).data('payment-type');
        var sum = $(elem).data('sum');
        var categoryId = $(elem).data('category-id');
        var comment = $(elem).data('comment');
        var place = $(elem).data('place');
        var date = $('#' + dayId).data('date');
        AddPaymentByDayHeader(paymentType, date, dayId, function () {
            $('#' + dayId + ' #paymentFormDiv .payment-input-sum').val(sum);
            $('#' + dayId + ' #paymentFormDiv .payment-input-category-id').val(categoryId);
            $('#' + dayId + ' #paymentFormDiv .payment-input-place').val(place);
            $('#' + dayId + ' #paymentFormDiv .payment-input-comment').val(comment);
        });
    });
}

function AddPaymentFast(el) {
    if ($(el).hasClass('init')) {
        return;
    }

    $(el).addClass('init');

    if ($('#paymentFastOperationTemplate').length == 0) {
        return;
    }
    var menu = $('#paymentFastOperationTemplate .dropdown-menu').clone();
    $('#addPaymentFastDiv .dropdown-menu').replaceWith(menu);

    $('#addPaymentFastDiv .fast-operation').click(function (e) {
        var elem = e.target;
        var paymentType = $(elem).data('payment-type');
        var sum = $(elem).data('sum');
        var categoryId = $(elem).data('category-id');
        var comment = $(elem).data('comment');
        var place = $(elem).data('place');
        AddPayment(paymentType, function () {
            $('#paymentFormBaseDiv .payment-input-sum').val(sum);
            $('#paymentFormBaseDiv .payment-input-category-id').val(categoryId);
            $('#paymentFormBaseDiv .payment-input-place').val(place);
            $('#paymentFormBaseDiv .payment-input-comment').val(comment);
            $('#paymentFormBaseDiv .payment-input-sum').select();
        });
    });
}

function ShowZero() {
    if ($('#showZeroChangeBtn').data('show-zero') == 0) {
        $('#showZeroChangeBtn').data('show-zero', 1);
        $('#showZeroChangeBtn i').addClass('fa-star-o');
        $('#showZeroChangeBtn i').removeClass('fa-star');
        $('#paymentsMain').removeClass("hide-zero");
    } else {
        $('#showZeroChangeBtn').data('show-zero', 0);
        $('#showZeroChangeBtn i').removeClass('fa-star-o');
        $('#showZeroChangeBtn i').addClass('fa-star');
        $('#paymentsMain').addClass("hide-zero");
    }
}