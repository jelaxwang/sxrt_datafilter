function checkExcel() {
    var aa = document.getElementsByName("fileUpload")[0].value.split(".");
    if (aa[aa.length - 1] != "xlsx") {
        alert("请选择excel2007极其以上版本文件.");
        event.returnValue = false;
    }
    else {
        document.getElementById("uploadTip").style.display = "block";
    }
}