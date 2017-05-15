var viewModel = {
    Login:function () {
        var username = $('#username').val();
        var password = $('#password').val();
        var txtCheckCode = $('#txtCheckCode').val();
        if(username==null||username.length==0){
            $('.message').html("用户名不能为空！");
        }else if(password==null||password.length==0){
            $('.message').html("密码不能为空！");
        } else if (txtCheckCode == null || txtCheckCode.length == 0) {
            $('.message').html("验证码不能为空！");
        } else {
               $.ajax({
                type: "POST",
                url: "/Login/doAction",
                data: { UserName: username, PassWord: password, txtCheckCode: txtCheckCode },
                dataType: "json",
                //contentType: "application/json",
                success: function (d) {
                    if (d.flag == 'Success') {
                        $('.message').html("登陆成功正在跳转，请稍候...");
                        window.location.href = '../User/Main.html';
                    } else {
                        $('.message').html(d.Message);
                    }
                },
                error: function (e) {
                    $('.message').html(e.responseText);
                },
                beforeSend: function () {
                    $('form').find("input").attr("disabled", true);
                    $('.message').html("正在登陆处理，请稍候...");
                },
                complete: function () {
                    $('form').find("input").attr("disabled", false);
                }
            });
        }
    },
    Reset: function () {
        document.getElementById('logform').reset();
    },
    GetCode: function (){
        document.getElementById("imgCheckCode").src = "/Login/CheckCode?rad=" + Math.random() + "";
    }
    };

$(function () {
    //登陆处理
    $('#login').click(function () {
        viewModel.Login();
    });
    //重置处理
    //$('#reset').click(function () {
    //    viewModel.Reset();
    //});
});