var umsDropdownPickerUrl = '/Reference/GetDropdownPickerValues';

function InitPicker(pickerId, isPreventInit) {
    var pickerType = $('#' + pickerId).data('picker-type');
    var showColumnNames = $('#' + pickerId).data('picker-show-column-names');
    var searchTemplate = $('#' + pickerId).data('picker-search-template');
    var additionalSearchTemplate = $('#' + pickerId).data('picker-additional-search-template');
    var orderTemplate = $('#' + pickerId).data('picker-order-template');
    var idColumnName = $('#' + pickerId).data('picker-id-column-name');
    var selectedvalues = $('#' + pickerId).data('default-selected'); //todo сейчас полностью объект кладём идентификатор и имя, а надо сделать чтоб приходил гуид а пикер обращался к справочнику и брал нужные по айдишничками
    var additionalValues = $('#' + pickerId).data('default-additional');
    var preventLoading = $('#' + pickerId).data('prevent-loading');
    var typeId = $('#' + pickerId).data('type-id');
    var offset = $('#' + pickerId).data('offset');
    var disableLoadNextButton = $('#' + pickerId).data('disable-load-next-button');
    var isHideSearch = $('#' + pickerId).data('is-hide-search');
    if (preventLoading == 0) {
        LocalLoadingStart('dropdownValuesBaseItem_' + pickerId);
        var selectAllText = $('#' + pickerId).data('selectall-text');
        $.ajax({
            type: "POST",
            url: umsDropdownPickerUrl,
            data: {
                pickerId: pickerId,
                pickerType: pickerType,
                searchTemplate: searchTemplate + additionalSearchTemplate,
                orderTemplate: orderTemplate,
                showColumnNames: showColumnNames,
                idColumnName: idColumnName,
                additionalValues: additionalValues,
                selectAllText: selectAllText,
                typeId: typeId,
                offset: offset,
                disableLoadNextButton: disableLoadNextButton,
                isHideSearch: isHideSearch
            },
            traditional: true,
            success: function (result) {
                if (result.success == false) {
                    ShowAlert('Внимание!', result.message, window.AlertType.Warning, 5000);
                } else {
                    $('#dropdownValues_' + pickerId).html(result);
                    DropDownPickerSelectValueDefault(pickerId);
                    DropDownPickerCheckSelectedValues(pickerId);
                    DropDownPickerSelectedSingleValue(pickerId);
                    LocalLoadingEnd('dropdownValuesBaseItem_' + pickerId);
                    var functionName = $("#dropDownPickerSelect" + pickerId).data('after-init-function');
                    if (functionName != undefined && functionName.length != 0) {
                        var fn = window[functionName];
                        if (typeof fn === "function") {
                            fn();
                        } else {
                            SystemConsoleLog("picker after init function: " + functionName + " not found", MessageType.Info);
                        }
                    }
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                ShowAlert('Ошибка!', window.commonErrorMessage, window.AlertType.Error, 2000);
            }
        }).always(LoadingEnd);
    } else {
        DropDownPickerSelectValueDefault(pickerId);
    }
}

function DropDownPickerMainDivClick(event) {
    var ev = event || window.event;
    ev.stopPropagation();
}

function DropDownPickerSearchKeyPress(e, pickerId) {
    if (e.keyCode == 13) {
        DropDownPickerSearchButtonClick(pickerId);
    }
}

function DropDownPickerSearchButtonClick(pickerId) {
    var searchStringId = 'DropDownPickerSearchInput_' + pickerId;
    var searchString = $('#' + searchStringId).val();
    DropDownPickerGetValues(pickerId, searchString);
}

var checkOnLoadSearchedValues = '';
function DropDownPickerLoadNextButtonClick(pickerId, searchString, offset, count) {
    DropDownPickerGetValues(pickerId, searchString, offset, count);
}

function DropDownPickerGetValues(pickerId, searchString, offset, count, elem) {
    LocalLoadingStart('dropdownValues_' + pickerId);
    var pickerType = $('#' + pickerId).data('picker-type');
    var searchTemplate = $('#' + pickerId).data('picker-search-template');
    var additionalSearchTemplate = $('#' + pickerId).data('picker-additional-search-template');
    var orderTemplate = $('#' + pickerId).data('picker-order-template');
    var showColumnNames = $('#' + pickerId).data('picker-show-column-names');
    var idColumnName = $('#' + pickerId).data('picker-id-column-name');
    var additionalValues = $('#' + pickerId).data('default-additional');
    var selectAllText = $('#' + pickerId).data('selectall-text');
    var typeId = $('#' + pickerId).data('typeId');
    LocalLoadingStart('dropdownValues_' + pickerId);
    $.ajax({
        type: "POST",
        url: umsDropdownPickerUrl,
        data: {
            pickerId: pickerId,
            pickerType: pickerType,
            searchTemplate: searchTemplate + additionalSearchTemplate,
            orderTemplate: orderTemplate,
            showColumnNames: showColumnNames,
            idColumnName: idColumnName,
            searchString: searchString,
            additionalValues: additionalValues,
            offset: offset,
            count: count,
            selectAllText: selectAllText,
            typeId: typeId
        },
        traditional: true,
        async: false,
        success: function (result) {
            if (result.success == false) {
                AlertErrorWindow("Внимание!", result.message);
            } else {
                $('#dropdownValues_' + pickerId).html(result);
                DropDownPickerCheckSelectedValues(pickerId);

                if (offset) {
                    $('#dropdownValues_' + pickerId).scrollTop($('#dropdownValues_' + pickerId)[0].scrollHeight);
                }
                $('#dropdownValues_' + pickerId).attr('data-search-value', $('#DropDownPickerSearchInput_' + pickerId).val());
            }
            LocalLoadingEnd('dropdownValues_' + pickerId);
        }
    });
}


function DropDownPickerCheckSelectedValues(pickerId) {
    for (var i = 0; i < $('#dropdownValues_' + pickerId + ' .dd-select-item input').length; i++) {
        var elem = $('#dropdownValues_' + pickerId + ' .dd-select-item input')[i];
        var selectedValue = $(elem).val();
        if (selectedValue == '') {
            selectedValue = "''";
        } else {
            selectedValue = '"' + selectedValue + '"';
        }

        if ($('#dropDownPickerSelect' + pickerId + ' option[value=' + selectedValue + ']').length) {
            $(elem).prop('checked', true);
            $(elem).closest('li').addClass('active');
        }
    }

    checkSelectAll(pickerId);
}

function DropDownPickerSelectAllValues(elem, pickerId, searchCount) {
    LocalLoadingStart('dropdownValues_' + pickerId);
    setTimeout(function () {
        DropDownPickerSelectAllValuesWithTimer(elem, pickerId, searchCount);
    }, 100);
}

function DropDownPickerSelectAllValuesWithTimer(elem, pickerId, searchCount) {
    var selectAllPressed = $('#dropdownValues_' + pickerId + ' .dd-select-all-id input').val();
    var downlodedLi = parseInt($('#dropdownValues_' + pickerId + ' .dd-select-all-id').attr('data-mapped-values-count'));
    if (selectAllPressed == 'false' || searchCount > downlodedLi) {
        var pickerType = $('#' + pickerId).data('picker-type');
        var searchTemplate = $('#' + pickerId).data('picker-search-template');
        var additionalSearchTemplate = $('#' + pickerId).data('picker-additional-search-template');
        var orderTemplate = $('#' + pickerId).data('picker-order-template');
        var showColumnNames = $('#' + pickerId).data('picker-show-column-names');
        var idColumnName = $('#' + pickerId).data('picker-id-column-name');
        var additionalValues = $('#' + pickerId).data('default-additional');
        var selectAllText = $('#' + pickerId).data('selectall-text');
        var searchString = $('#dropdownValues_' + pickerId).attr('data-search-value');
        var offset = 0;
        var count = searchCount;
        $.ajax({
            type: "POST",
            url: umsDropdownPickerUrl,
            data: {
                pickerId: pickerId,
                pickerType: pickerType,
                searchTemplate: searchTemplate + additionalSearchTemplate,
                orderTemplate: orderTemplate,
                showColumnNames: showColumnNames,
                idColumnName: idColumnName,
                searchString: searchString,
                additionalValues: additionalValues,
                offset: offset,
                count: count,
                selectAllText: selectAllText
            },
            traditional: true,
            success: function (result) {
                if (result.success == false) {
                    AlertErrorWindow("Внимание!", result.message);
                } else {
                    $('#dropdownValues_' + pickerId).attr('data-search-value', $('#DropDownPickerSearchInput_' + pickerId).val());
                    $('#dropdownValues_' + pickerId).html(result);
                    if (selectAllPressed == 'false') {
                        checkedAll(elem, pickerId, true);
                    } else {
                        checkedAll(elem, pickerId, false);
                    }
                }
                LocalLoadingEnd('dropdownValues_' + pickerId);
            }
        });
    } else {
        checkedAll(elem, pickerId, false);
        LocalLoadingEnd('dropdownValues_' + pickerId);
    }
}

function checkedAll(elem, pickerId, checked) {
    var isSingle = $("#dropDownPickerSelect" + pickerId).data('single-select') == 1;
    if (!isSingle) {
        if (!checked) {
            //todo selectedValue если будет значение из нескольких строк, трабла будет, нужно будет в ковычки взять пример: value='bob 217'
            //if ($("#dropDownPickerSelect" + pickerId + " option").length) {
            //    $("#dropDownPickerSelect" + pickerId + " option").remove();
            //}
            $('#dropdownValues_' + pickerId + ' .dd-select-item input').each(function () {
                var val = $(this).val();
                if (val == '') {
                    val = "''";
                } else {
                    val = '"' + val + '"';
                }

                for (var i = 0; i < $('#dropDownPickerSelect' + pickerId + ' option[value=' + val + ']').length; i++) {
                    $($('#dropDownPickerSelect' + pickerId + ' option[value=' + val + ']')[i]).remove();
                }
                $(this).prop('checked', false);
                $(this).closest('li').removeClass('active');
            });

            $('#dropdownValues_' + pickerId + ' .dd-select-all-id input').val(false);
            $('#dropdownValues_' + pickerId + ' .dd-select-all-id').removeClass('active');
        } else {
            $('#dropdownValues_' + pickerId + ' .dd-select-item input').each(function () {
                var val = $(this).val();
                var txt = $(this).data('show-value');
                if (val == '') {
                    val = "''";
                } else {
                    val = '"' + val + '"';
                }
                if (!$('#dropDownPickerSelect' + pickerId + ' option[value=' + val + ']').length) {
                    $("#dropDownPickerSelect" + pickerId).append("<option value=" + val + " selected>" + txt + "</option>");
                }
                $(this).prop('checked', true);
                $(this).closest('li').addClass('active');
            });

            $('#dropdownValues_' + pickerId + ' .dd-select-all-id input').val(true);
            $('#dropdownValues_' + pickerId + ' .dd-select-all-id').addClass('active');
        }
        DropDownPickerUpdateSelectedText(pickerId);
        var functionName = $("#dropDownPickerSelect" + pickerId).data('after-select-function');
        if (functionName != undefined && functionName.length != 0) {
            var fn = window[functionName];
            if (typeof fn === "function") {
                var value = DropDownPickerGetValue(pickerId);
                fn(value);
            } else {
                SystemConsoleLog("picker after select function: " + functionName + " not found", MessageType.Info);
            }
        }
    } else {
        console.error("using select all in single select is INCORRECT!!!");
    }
};

function checkSelectAll(pickerId) {
    if ($("#dropDownPickerSelect" + pickerId + " option").length == $("#idItemsCountOf_" + pickerId).data("total-count")) {
        $('#dropdownValues_' + pickerId + ' .dd-select-all-id').addClass('active');
        $('#dropdownValues_' + pickerId + ' .dd-select-all-id input').val(true);
    }
}

function DropDownPickerSelectValue(elem, pickerId) {
    var functionName = $("#dropDownPickerSelect" + pickerId).data('before-select-function');
    if (functionName != undefined && functionName.length != 0) {
        var fn = window[functionName];
        if (typeof fn === "function") {
            var params = $("#dropDownPickerSelect" + pickerId).data('before-select-function-param');
            var checked = $(elem).prop('checked');
            $(elem).prop('checked', !checked);
            var selectedValue = $(elem).val();
            fn(selectedValue, checked, params, function () {
                $(elem).prop('checked', checked);
                DropDownPickerSelectValueBody(elem, pickerId);
            }, elem);
        } else {
            SystemConsoleLog("picker before select function: " + functionName + " not found", MessageType.Info);
        }
    } else {
        DropDownPickerSelectValueBody(elem, pickerId);
    }
}

function DropDownPickerSelectValueBody(elem, pickerId) {
    var i;
    var selectedValue = $(elem).val();
    if (selectedValue == '') {
        selectedValue = "''";
    } else {
        selectedValue = '"' + selectedValue + '"';
    }
    var checked = $(elem).prop('checked');
    var isSingle = $("#dropDownPickerSelect" + pickerId).data('single-select') == 1;
    var isSingleNotNullable = $("#dropDownPickerSelect" + pickerId).data('single-not-nullable') == 1;
    var isChangeSelectedValues = true;
    if (!checked) { //todo selectedValue если будет значение из нескольких строк, трабла будет, нужно будет в ковычки взять пример: value='bob 217'
        if (isSingle && isSingleNotNullable) {
            $(elem).prop('checked', true);
            isChangeSelectedValues = false;
        } else {
            if (!isSingle) {
                if ($('#dropdownValues_' + pickerId + ' .dd-select-all-id input').val() == 'true') {
                    $('#dropdownValues_' + pickerId + ' .dd-select-all-id').removeClass('active');
                    $('#dropdownValues_' + pickerId + ' .dd-select-all-id input').val(false);
                }
            }
            if ($("#dropDownPickerSelect" + pickerId + " option[value=" + selectedValue + "]").length) {
                $("#dropDownPickerSelect" + pickerId + " option[value=" + selectedValue + "]")[0].remove();
            }
            for (i = 0; i < $('#dropdownValues_' + pickerId + ' input[value=' + selectedValue + ']').length; i++) {
                $($('#dropdownValues_' + pickerId + ' input[value=' + selectedValue + ']')[i].closest('li')).removeClass('active');
                $($('#dropdownValues_' + pickerId + ' input[value=' + selectedValue + ']')[i]).prop('checked', false);
            }
        }
    } else {
        if (isSingle) {
            $("#dropDownPickerSelect" + pickerId + " option").remove();
            $('#dropdownValues_' + pickerId + ' input:checked').closest('li').removeClass('active');
            $('#dropdownValues_' + pickerId + ' input:checked').prop('checked', false);
        }
        var text2 = $(elem).data('show-value');
        $("#dropDownPickerSelect" + pickerId).append("<option value=" + selectedValue + " selected>" + text2 + "</option>");
        for (i = 0; i < $('#dropdownValues_' + pickerId + ' input[value=' + selectedValue + ']').length; i++) {
            $($('#dropdownValues_' + pickerId + ' input[value=' + selectedValue + ']')[i]).prop('checked', true);
            $($('#dropdownValues_' + pickerId + ' input[value=' + selectedValue + ']')[i]).closest('li').addClass('active');
        }

        if (!isSingle) {
            checkSelectAll(pickerId);
        }
    }
    if (isChangeSelectedValues) {
        DropDownPickerUpdateSelectedText(pickerId);
        var closeAfterSelect = $('#' + pickerId).data('close-after-select');
        if (closeAfterSelect == 1) {
            $('#' + pickerId).parent().removeClass('open');
        }
        CheckFunctionAfterSelect(pickerId);
    }
}

function CheckFunctionAfterSelect(pickerId) {
    var functionName = $("#dropDownPickerSelect" + pickerId).data('after-select-function');
    if (functionName != undefined && functionName.length != 0) {
        var fn = window[functionName];
        if (typeof fn === "function") {
            var params = $("#dropDownPickerSelect" + pickerId).data('after-select-function-param');
            var value = DropDownPickerGetValue(pickerId);
            fn(value, params);
        } else {
            SystemConsoleLog("picker after select function: " + functionName + " not found", MessageType.Info);
        }
    }
}

function DropDownPickerSelectValueDefault(pickerId) {
    var selectedValues = $('#' + pickerId).data('default-selected');
    if (selectedValues != '' && selectedValues != undefined) {

        // todo: Когда будет рефакторинг этого места, то вот вполне работающий метод
        //var pickerType = $('#' + pickerId).data('picker-type');
        //var showColumnNames = $('#' + pickerId).data('picker-show-column-names');
        //var typeId = $('#' + pickerId).data('type-id');
        //$.ajax({
        //    type: "POST",
        //    url: '/Documents/GetDropdownPickerDefaultValues',
        //    data: {
        //        pickerType: pickerType,
        //        searchString: selectedValues,
        //        showColumnNames: showColumnNames,
        //        typeId: typeId
        //    },
        //    traditional: true,
        //    success: function (result) {
        //        if (result.success == false) {
        //            ShowAlert('Внимание!', result.message, window.AlertType.Warning, 5000);
        //        } else {
        //            DropDownPickerClearSelected(pickerId);
        //            var selectedRemoveIfFoundNotExists = $('#' + pickerId).data('default-selected-remove-if-found-not-exists');
        //            var myRe = /[{] Value = .*?[,] Name = .*? [}]/g;;
        //            var values = result.values.match(myRe);
        //            if (values == undefined)
        //                return;
        //            var selectedValues = [];
        //            for (var i = 0; i < values.length; i++) {
        //                var valueIndex = values[i].indexOf("{ Value = ");
        //                var nameIndex = values[i].indexOf(", Name = ");
        //                var value = values[i].substring(valueIndex + 10, nameIndex); //"{ Value = " - length 10
        //                var name = values[i].substring(nameIndex + 9, values[i].length - 2); //", Name = " - length 9

        //                selectedValues.push(value);

        //                var text2 = $('#dropdownValues_' + pickerId + ' input[value=' + value + ']').data('show-value');
        //                if (text2 != undefined) {
        //                    name = text2;
        //                    $("#dropDownPickerSelect" + pickerId).append("<option value=" + value + " selected>" + name + "</option>");
        //                } else {
        //                    if (!selectedRemoveIfFoundNotExists) {
        //                        $("#dropDownPickerSelect" + pickerId).append("<option value=" + value + " selected>" + name + "</option>");
        //                    }
        //                }
        //            }
        //            for (var j = 0; j < selectedValues.length; j++) {
        //                $($('#dropdownValues_' + pickerId + ' input[value=' + selectedValues[j] + ']')).prop('checked', true);
        //                $($('#dropdownValues_' + pickerId + ' input[value=' + selectedValues[j] + ']')).closest('li').addClass('active');
        //            }
        //            $('#' + pickerId).data('default-selected', '');
        //            DropDownPickerUpdateSelectedText(pickerId);
        //            CheckFunctionAfterSelect(pickerId);
        //        }
        //    },
        //    error: function(xhr, textStatus, errorThrown) {
        //        ShowAlert('Ошибка!', window.commonErrorMessage, window.AlertType.Error, 2000);
        //    }
        //});

        DropDownPickerClearSelected(pickerId);
        var selectedRemoveIfFoundNotExists = $('#' + pickerId).data('default-selected-remove-if-found-not-exists');
        var myRe = /[{] Guid = .*?[,] Name = .*? [}]/g;
        var values = selectedValues.match(myRe);
        if (values == undefined)
            return;
        var selectedGuids = [];
        for (var i = 0; i < values.length; i++) {
            var valueIndex = values[i].indexOf("{ Guid = ");
            var nameIndex = values[i].indexOf(", Name = ");
            var value = values[i].substring(valueIndex + 9, nameIndex); //"{ Guid = " - length 9
            var name = values[i].substring(nameIndex + 9, values[i].length - 2); //", Name = " - length 9

            selectedGuids.push(value);

            var text2 = $('#dropdownValues_' + pickerId + ' input[value="' + value + '"]').data('show-value');
            if (text2 != undefined) {
                name = text2;
                $("#dropDownPickerSelect" + pickerId).append("<option value=\"" + value + "\" selected>" + name + "</option>");
            } else {
                if (!selectedRemoveIfFoundNotExists) {
                    $("#dropDownPickerSelect" + pickerId).append("<option value=\"" + value + "\" selected>" + name + "</option>");
                }
            }
        }

        for (var j = 0; j < selectedGuids.length; j++) {
            $($('#dropdownValues_' + pickerId + ' input[value="' + selectedGuids[j] + '"]')).prop('checked', true);
            $($('#dropdownValues_' + pickerId + ' input[value="' + selectedGuids[j] + '"]')).closest('li').addClass('active');
        }
        $('#' + pickerId).data('default-selected', '');
        DropDownPickerUpdateSelectedText(pickerId);
        CheckFunctionAfterSelect(pickerId);
    }
}

function DropDownPickerSelectedSingleValue(pickerId) {
    var selectedvalue = $('#' + pickerId).data('single-default-selected');
    if (selectedvalue != undefined && selectedvalue != "") {
        var name = $('#dropdownValues_' + pickerId + ' input[value=' + selectedvalue + ']').data('show-value');
        if (name != undefined) {
            $("#dropDownPickerSelect" + pickerId).append("<option value=" + selectedvalue + " selected>" + name + "</option>");
        }
        $($('#dropdownValues_' + pickerId + ' input[value=' + selectedvalue + ']')).prop('checked', true);
        $($('#dropdownValues_' + pickerId + ' input[value=' + selectedvalue + ']')).closest('li').addClass('active');
        DropDownPickerUpdateSelectedText(pickerId);
    }
}

function DropDownPickerUpdateSelectedText(pickerId) {
    var disableShowSelectedText = $('#' + pickerId).data('disable-show-selected-text');;
    if (disableShowSelectedText == 0) {
        var showSelectMax = $('#' + pickerId).data('show-select-max');
        //var pickerSelected = $("#dropDownPickerSelect" + pickerId + " option");
        var pickerSelectedCount = $("#dropDownPickerSelect" + pickerId + " option").size();
        var text;
        var defaultText = $('#' + pickerId).data('default-text');
        if (pickerSelectedCount > showSelectMax) {

            var countValues = $("#idItemsCountOf_" + pickerId).data('countValues');
            if (pickerSelectedCount == countValues) {
                var sName = $("#" + pickerId).data('selectall-text');
                text = sName;
            } else {
                text = "Выбрано " + pickerSelectedCount;
            }
        } else if (pickerSelectedCount < 1) {
            text = defaultText;
        } else {
            text = '';
            for (var i2 = 0; i2 < pickerSelectedCount; i2++) {
                var val2 = $("#dropDownPickerSelect" + pickerId + " option")[i2].text;
                if (i2 != 0) {
                    text += ', ';
                }
                text += val2;
            }
        }
        $('#' + pickerId + ' span').text(text);
        var hasSelectedClass = $("#dropDownPickerSelect" + pickerId).data('container-class-if-has-selected');
        if (hasSelectedClass != undefined && hasSelectedClass.length != 0 && pickerSelectedCount > 0) {
            $('#' + pickerId).parent().addClass(hasSelectedClass);
        } else {
            $('#' + pickerId).parent().removeClass(hasSelectedClass);
            $('#' + pickerId).removeClass('red-border');
        }
    }
    var showSelectedTitle = $('#' + pickerId).data('show-selected-title');
    var selectedTitlePrefix = $('#' + pickerId).data('selected-title-prefix');
    if (showSelectedTitle == 1) {

        if (pickerSelectedCount > 0) {
            var title = '';
            if (selectedTitlePrefix.length > 0) {
                title += selectedTitlePrefix + '\u000d';
            }
            for (var i = 0; i < pickerSelectedCount; i++) {
                var val = $("#dropDownPickerSelect" + pickerId + " option")[i].text;
                if (i != 0) {
                    title += '\u000d';
                }
                title += val;
            }
            $('#' + pickerId).prop('title', title);
        } else {
            $('#' + pickerId).prop('title', defaultText);
        }
    }
}

function DropDownPickerGetSelectedText(pickerId) {
    return $('#' + pickerId + ' span').text();
}

function DropDownPickerSetValue(pickerId, value, valueText) {
    DropDownPickerClearSelected(pickerId);
    $("#dropDownPickerSelect" + pickerId).append("<option value=\"" + value + "\" selected>" + valueText + "</option>");
    $($('#dropdownValues_' + pickerId + ' input[value="' + value + '"]')).prop('checked', true);
    $($('#dropdownValues_' + pickerId + ' input[value="' + value + '"]')).closest('li').addClass('active');
    $('#' + pickerId + ' span').text(valueText);
    $('#' + pickerId).data('default-selected', '');
    CheckFunctionAfterSelect(pickerId);
}

function DropDownPickerClearSelected(pickerId) {
    $("#dropdownValues_" + pickerId + " .dd-select-all-id input").val(false);
    $("#dropdownValues_" + pickerId + " .dd-select-all-id").removeClass('active');
    $("#dropDownPickerSelect" + pickerId + " option").remove();
    $('#dropdownValues_' + pickerId + ' input:checked').closest('li').removeClass('active');
    $('#dropdownValues_' + pickerId + ' input:checked').prop('checked', false);
    var text = $('#' + pickerId).data('default-text');
    $('#' + pickerId + ' span').text(text);
}

function DropDownPickerGetText(pickerId) {
    var text = $('#' + pickerId + ' span').text();
    return text;
}

function DropDownPickerGetValueEmptyIfNull(pickerId) {
    var val = DropDownPickerGetValue(pickerId);
    if (val == null) {
        val = [];
    }
    return val;
}

function DropDownPickerGetValue(pickerId) {
    var val = $("#dropDownPickerSelect" + pickerId).val();
    if (val == null) {
        return null;
    }
    var isSingle = $("#dropDownPickerSelect" + pickerId).data('single-select') == 1;
    if (isSingle) {
        return val[0];
    } else {
        return val;
    }
}
function DropDownPickerGetAllLoadedValues(pickerId) {
    var val = [];
    $('#dropdownValues_' + pickerId + ' .dd-select-item input').each(function (index, elem) {
        val.push($(elem).val());
    });
    return val;
}

function OpenDropDown(elem, event) {
    if ($(elem).parent().hasClass('disabled')) {
        return;
    }
    event = event || window.event;
    var pickerId = $(elem).attr('id');
    var preventLoading = $(elem).data('prevent-loading');
    if (preventLoading == 1) {
        $(elem).data('prevent-loading', '0').attr('data-prevent-loading', '0');
        window.InitPicker(pickerId);
    }
    if ($(elem).parent().hasClass('open')) {
        $(elem).parent().removeClass('open');
    } else {
        var container = $(".ums-picker-btn").parent();
        container.removeClass('open');
        $(elem).parent().addClass('open');
    }
    var id = $(elem).attr("id");
    var topfilterwidth = $('.main-content').width();
    var ulwidth = $(elem).width();
    var widthwihcoordinate = event.clientX + ulwidth;
    var error = 100;
    var ulId = '#dropdownValues_' + id;
    if (topfilterwidth < (widthwihcoordinate)) {
        $(ulId).addClass('pull-right');
    } else {
        $(ulId).removeClass('pull-right');
    }
}

$(document).mousedown(function (e) {
    var container = $(".ums-picker-btn").parent();
    if (container.has(e.target).length === 0) {
        container.removeClass('open');
    }
});

