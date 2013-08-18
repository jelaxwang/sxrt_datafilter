var perConditionDesc = "";
var strTypes = new Array("char", "varchar", "nchar", "nvarchar");
var numnberTypes = new Array("int", "decimal", "float");

function appendFilter(filterDesc, $obj) {
    $obj.append(filterDesc);
    perConditionDesc = filterDesc;
}

function resetOperation(domElement) {
    var stringOperation = "<select style='width: 150px;' operationType='string'><option value='='>等于</option><option value='like'>约等于</option><option value='in'>In[多个数据用,隔开]</option></select>";
    var numOperation = "<select style='width: 150px' operationType='number'><option value='='>等于</option><option value='>'>大于</option><option value='<'>小于</option><option value='>='>大于等于</option><option value='<='>小于等于</option><option value='in'>In[多个数据用,隔开]</option></select>";
    var dataTimeOperation = "<select style='width: 150px' operationType='datatime'><option value='='>等于</option><option value='>'>大于</option><option value='<'>小于</option><option value='>='>大于等于</option><option value='<='>小于等于</option></select>";

    /*
    生成 Operation 控件规则:
    如果obj是string类型的,则用stringOperation生成控件,将生成的控件替换原来的<select>,主要用index确定原来的<select>
    如果obj是number类型的,则用numOperation生成控件,将生成的控件替换原来的<select>,主要用index确定原来的<select>
    如果obj是datetime类型的,dataTimeOperation,将生成的控件替换原来的<select>,主要用index确定原来的<select>

    如果当前选的和上一次选的都属于同一类,则就不用在重新生成Operation 控件,Operation 控件 直接使用上一次的结果
    */
    var $obj = $(domElement); //选择的字段下拉框
    var $parent = $(domElement.parentElement); //每一个panel
    var $oldOperation = $($parent.children()[1]); //操作下拉框
    var currentOperationType = $oldOperation.attr("operationType"); //操作下拉框是否针对字符类型

    /* ======operation 控制 ========== */
    if ($obj.val().toLocaleLowerCase().isIn(strTypes)) {
        if (currentOperationType == undefined || currentOperationType != "string") {
            $oldOperation.remove();
            //用stringOperation生成控件
            var $newOperation = $(stringOperation);
            $newOperation.insertAfter($parent.children()[0]);
        }
    }
    else if ($obj.val().toLocaleLowerCase() == "datetime") {
        if (currentOperationType == undefined || currentOperationType != "datetime") {
            $oldOperation.remove();
            //用dataTimeOperation生成控件
            var $newOperation = $(dataTimeOperation);
            $newOperation.insertAfter($parent.children()[0]);
        }
    }
    else if ($obj.val().toLocaleLowerCase().isIn(numnberTypes)) {
        if (currentOperationType == undefined || currentOperationType != "number") {
            $oldOperation.remove();
            //用numOperation生成控件
            var $newOperation = $(numOperation);
            $newOperation.insertAfter($parent.children()[0]);
        }
    }

    /* ==============输入框控制=============== */
    var $txtInput = $($parent.children()[2]);
    $txtInput.val("");
    if ($obj.val().toLocaleLowerCase() == "datetime") {
        $txtInput.attr("readonly", "true").datepicker();
    }
    else {
        $txtInput.removeAttr("readonly");
        $txtInput.datepicker("destroy");
    }
}

function addFilter(domElement) {
    var $newFilter = $(perConditionDesc);
    $newFilter.insertAfter(domElement);
    $newFilter.children().first().trigger("change");
}

function deleteFilter(baseElement) {
    if (baseElement.parentElement.children.length == 1) {
        window.alert("至少要有一个筛选条件");
        return;
    }
    $(baseElement).remove();
}

function checkInput(domElement) {
    var $fieldFilterObj = $(domElement).parent().children().first();
    var operationSign = $($(domElement).parent().children().get(1)).val();
    var fieldName = $fieldFilterObj.find("option:selected").text();
    var fieldType = $fieldFilterObj.val().toLocaleLowerCase();
    var inputValue = $.trim(domElement.value);
    if (inputValue.length > 0) {
        if (fieldType.isIn(numnberTypes)) {

            var tempInputValue = operationSign == "in" ? inputValue.replace(/\s*,\s*/g, "") : inputValue;
            if (fieldType == "int" && !tempInputValue.isInteger()) {
                window.alert(fieldName + "是整数类型.");
                domElement.focus();
                domElement.select();
                return;
            }
            if (fieldType.isIn(new Array("decimal", "float")) && !tempInputValue.isFloat()) {
                window.alert(fieldName + "是浮点类型.");
                domElement.focus();
                domElement.select();
                return;
            }
        }
    }
}

function moveSelectedOption(sourceId, targetId) {
    var $sourceSelectedOptions = $("#" + sourceId + " :selected");
    var $target = $("#" + targetId);
    $sourceSelectedOptions.appendTo($target);
    $target[0].selectedIndex = -1;
}

/*
filterTabId: 过滤字段的tab Id
fieldslistBoxId: 输出列Box Id
isB2BData:是否是B2B数据
*/
function getQueryParams(filterTabId, fieldslistBoxId, isB2BData) {
    var $filterTab = $("#" + filterTabId);
    var $fieldslistBox = $("#" + fieldslistBoxId);

    var filters = new Array();
    $filterTab.children().each(function (itemIndex) {
        var filterFieldTableName = $(this).children().first().find("option:selected").attr("t");
        var filterFieldType = $(this).children().first().val().toLocaleLowerCase();
        var filterFieldName = $(this).children().first().find("option:selected").text();
        var operationSign = $($(this).children().get(1)).val();
        var inputValue = $.trim($(this).children().get(2).value);

        if (inputValue.length > 0) {
            if (operationSign == "like") {
                inputValue = "'%" + inputValue + "%'";
            }
            else if (operationSign == "in") {
                if (filterFieldType.isIn(strTypes) || filterFieldType == "datetime") {
                    if (",".isIn(inputValue)) {
                        inputValue = inputValue.replace(/\s*,\s*/g, "','");
                        inputValue = "('" + inputValue + "')";

                    }
                    else {
                        inputValue = "('" + inputValue + "')";
                    }
                }
                else {
                    if (",".isIn(inputValue)) {
                        inputValue = inputValue.replace(/\s*,\s*/g, ",");
                        inputValue = "(" + inputValue + ")";

                    }
                    else {
                        inputValue = "(" + inputValue + ")";
                    }
                }
            }
            else {
                if (filterFieldType.isIn(strTypes) || filterFieldType == "datetime") {
                    inputValue = "'" + inputValue + "'";
                }
            }


            filters.push({ "FieldName": filterFieldTableName + "." + filterFieldName, "OperationSign": operationSign, "InputValue": inputValue });
        }
    });

    var fieldsStr = "";
    $fields = $("#" + fieldslistBoxId);
    $fields.children().each(function (item) {
        fieldsStr = fieldsStr + "," + $(this).attr("t") + "." + $(this).text();
    });

    fieldsStr = fieldsStr.substr(1);

    return { "Filters": filters, "Columns": fieldsStr };
}

/*
filterTabId: filter table Id
fieldslistBoxId: 输出列的listbox Id
isB2BData: 是否是B2B数据
isPreview: 是否是预览
*/
function exportData(filterTabId, fieldslistBoxId, isB2BData, isPreview) {
    var queryParams = getQueryParams(filterTabId, fieldslistBoxId, isB2BData);

    if (queryParams.Filters.length <= 0) {
        alert("请选择筛选条件");
        return;
    }

    if (queryParams.Columns.length <= 0) {
        alert("请选择输出列");
        return;
    }

    if ($.trim(isPreview).toLocaleLowerCase() == "true")
        previewData(queryParams, isB2BData);
    else
        requstServerToExportData(queryParams, isB2BData);
}

function requstServerToExportData(requestData, isB2BData) {
    var excelFileName = "";
    $.ajax({
        url: "/Export/ExportData?isB2BData=" + isB2BData,
        data: requestData,
        async: false,
        type: "POST",
        success: function (fileName) {
            excelFileName = fileName;
        },
        error: function () { alert("export error"); }
    });

    if (excelFileName == "error-nodata") {
        alert(excelFileName);
    }
    else {
        window.open("/Export/DownLoadExcel?fileName=" + excelFileName);
    }
}

function previewData(requestData, isB2BData) {
    //用返回的数据在#dialog-modal-preview 中构建flexigrid插件,flexigrid 插件是要能够自动加载数据的
    //    $.ajax({
    //        url: "/Export/PreviewData?isB2BData=" + isB2BData,
    //        data: requestData,
    //        async: false,
    //        type: "POST",
    //        context: $("#progressPanel")[0],
    //        dataType: "json",
    //        beforeSend: function () {
    //            $(this).show();
    //        },
    //        complete: function () {
    //            $(this).hide();
    //        },
    //        success: function (jsonData) {
    //            $("#previewDataTb").flexigrid({

    //            });



    //            //            $("#dialog-modal-preview").dialog({
    //            //                height: 500,
    //            //                modal: true
    //            //            });
    //        },
    //        error: function () { alert("get preview data error"); }
    //    });
}
