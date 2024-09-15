function LoadingStart(loadingString) {
    if ($('body').length) {
        if (loadingString != undefined && loadingString.length > 0) {
            $('body').append('<div id="parent_loading"><div id="loading_center"><div id="loading_center"></div><div style = "display: inline;"><span style="color: white;">' + loadingString + '</span></div></div><div>');
        } else {
            $('body').append('<div id="parent_loading"><div id="loading_center"><div id="loading_body"></div></div><div>');
        }
    }
}

function LoadingEnd() {
    $('#parent_loading').remove();
}


function LocalLoadingStart(id) {
    if ($('#' + id).length) {
        $('#' + id).append('<div class="loading-parent-2" ><div class="center"><div class="loading-2"></div></div><div>');
    }
}

function LocalLoadingEnd(id) {
    if ($('#' + id + ' .loading-parent-2').length) {
        $('#' + id + ' .loading-parent-2').remove();
    }
}
//picker


function InitDatePicker(elem) {
    if ($(elem).hasClass('need-init-picker')) {
        $(elem).datetimepicker({
            format: 'DD.MM.YYYY',
            language: 'ru',
        });
        $(elem).removeClass('need-init-picker');
    }
}

//alert
var commonErrorMessage = "Повторите операцию позже или обратитесь к администратору";

function _enum_(_name, Elems) {
    var elem, value;
    window[_name] = {};
    for (var i = Elems.length; i--;) {
        elem = Elems[i];
        value = elem.replace(/\s/g, '').split('=');
        window[_name][value[0]] = {
            value: value[0],
            int: value[1] | i,
            toString: function () {
                return this.value;
            }
        };
    }
}
_enum_('AlertType', ['Warning', 'Error', 'Info', 'Success']);
var alertRebornNumber = 0;
function ShowAlert(title, message, type, time, isNeedTime, needId) {
    if (time == undefined || time < 0) {
        time = 55000;
    }
    var alertClass = '';
    switch (type) {
        case window.AlertType.Warning:;
            alertClass = 'alert-warning';
            break;
        case window.AlertType.Error:
            alertClass = 'alert-danger';
            break;
        case window.AlertType.Info:
            alertClass = 'alert-info';
            break;
        case window.AlertType.Success:
            alertClass = 'alert-success';
            break;
        default:
            alertClass = 'alert-info';
            break;
    }
    if (!$('#alert-body').length) {
        $('body').append('<div id="alert-body" class="reborn-alert"></div>');
    }
    if (title == undefined) {
        title = 'Уведомление';
    }
    alertRebornNumber++;
    var id = 'alert-' + alertRebornNumber;
    if (needId != undefined && needId != "") {
        id = 'alert-' + needId;
    }
    var text = '<div id="' + id + '" class="alert alert-block ' + alertClass + ' ">' +
        '<a class="close" data-dismiss="alert" href="#">X</a>' +
        '<h4 class="alert-heading">' +
        '<span class="fa fa-warning"></span>' + title +
        '</h4>' +
        '<label>' + message + '</label>' +
        '</div>';
    $('#alert-body').append(text);
    if (isNeedTime == undefined || isNeedTime == true) {
        setTimeout(function () {
            $('#' + id).remove();
        }, time);
    }

}

_enum_('MessageType', ['Warning', 'Error', 'Info', 'Success']);
function SystemConsoleLog(message, type) {
    if (type == undefined) {
        type = window.MessageType.Info;
    }
    var typeTxt = "";
    switch (type) {
        case window.AlertType.Warning:;
            typeTxt = 'warning';
            break;
        case window.AlertType.Error:
            typeTxt = 'danger ';
            break;
        case window.AlertType.Info:
            typeTxt = 'info   ';
            break;
        case window.AlertType.Success:
            typeTxt = 'success';
            break;
        default:
            typeTxt = 'info   ';
            break;
    }
    var dateStr = GetCurrentDateString();
    console.log(dateStr + " | " + typeTxt + " | " + message);
}

function GetCurrentDateString() {
    var dt = new Date();
    var hh = dt.getHours();
    var mm = dt.getMinutes();
    var ss = dt.getSeconds();
    var n = dt.getMilliseconds();

    hh = hh < 10 ? '0' + hh : hh;
    mm = mm < 10 ? '0' + mm : mm;
    ss = ss < 10 ? '0' + ss : ss;
    n = n < 10 ? '0' + n : n;
    n = n < 100 ? '0' + n : n;
    var str = hh + ':' + mm + ':' + ss + '.' + n;
    return str;
}

function GetRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}
