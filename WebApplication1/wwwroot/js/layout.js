$(document).ready(function () {
    $('#c_1').hide(0);
});
$("#er").mouseout(function () {
    $("#c_1").hide("fast");
});
$("#dtn_go_test").click(function () {
    var id = document.getElementById("test_kod").value;
    $.ajax({
        type: 'POST',
        url: '/test/GetTest?id=' + id,
        value: id,
        dataType: 'json',
        success: function (data) {
            if (data === "Error") {
                $("#er").empty();
                $("#er").append($('<p>',
                    {
                        'id': 'c_1',
                        'text': 'Неверный код'
                    }));
                $("#er").css('color', 'red');
            }
            else if (data.value === 'true') {
                $("#er").empty();
                $("#er").append($('<p>',
                    {
                        'id': 'c_1',
                        'text': 'Тест уже был пройден'
                    }));
                $("#er").css('color', 'red');
            }
            else if (data !== "Error") {
                document.getElementById("caren").remove();
                window.location.href = '/test/GetPassage?id=' + id;
            }
        }
    });
});