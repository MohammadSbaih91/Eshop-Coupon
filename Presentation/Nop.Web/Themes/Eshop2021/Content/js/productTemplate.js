(function (productTemplate, $, undefined) {
    //#region  common variable
    var startFormMonth = "";
    var adControlPayment = 0;
    //#endregion

    //#region
    productTemplate.fncallCommonDocumentReady = function (controlIdArg, startFormMonthArg, productpriceArg) {
        var productPrice = 0;
        adControlPayment = controlIdArg;
        startFormMonth = startFormMonthArg;
        productPrice = productpriceArg;

        //#region Product With Plan Document Ready

        $('#divdeviceprice').attr("data-productPrice", productPrice);
        productTemplate.fnMobileSim(productPrice, 0, 0,false);

        //#endregion

        //#region Plan With  Product  Document Ready

        $('#productPrice').attr("data-productprice", productPrice);
        fnAjaxCall(productPrice, 0);

        //#endregion
    };
    //#endregion

    //#region Product With Plan

    productTemplate.fnMobileSim = function (planPrice, planPriceValue, selectedplanid,isSimType) {
        var adPayment = adControlPayment;
        var productId = $('#hdnProductId').val();
        fnLoadProductSimcard(selectedplanid);
        var ddlPriceAttributevalueId = $("#ddlPriceAttribute_" + selectedplanid).attr("data-selectid");
        $("#lblPaymentInstallments").html($("#ddlPriceAttribute_" + selectedplanid).html());
        $("#hdnSelectedAttributeValueId").val(ddlPriceAttributevalueId);
        var selectedAttributePrice = $('#hdnpriceattribute_' + selectedplanid).val();
        $('#hdnSelectedAttributeValuePrice_' + selectedplanid).val(selectedAttributePrice);
        if (planPriceValue > 0) {
            $('#trSimPrice').show();
            $('#simPrice').html(planPrice + " <strong>/" + startFormMonth + "</strong>");
        }
        else {
            $('#trSimPrice').hide();
        }
        $('#simPrice').attr("data-simPrice", planPriceValue);
        var productPrice = $('#divdeviceprice').attr("data-productPrice");
        $(".bun").removeClass("selected");
        $(".clsProduct_" + selectedplanid).addClass("selected");
        $.ajax({
            url: '/shoppingcart/productdetails_addplan',
            data: 'productPrice=' + parseFloat(productPrice) + '&planPrice=' + parseFloat(planPriceValue) + '&advancePayment=' + adPayment + '&selectedAttributeValue=' + parseFloat(selectedAttributePrice),
            type: 'post',
            success: function (data) {
                $('#divdeviceprice').html(data.deviceFormatedPrice);
                $('#divTotalPrice').html(data.subTotalFormatedPrice);
                $('#spnTotalFullPrice').html(data.formatedTotalFullPrice);
                $('#spnSimFullPrice').html(data.formatedPlanFullPrice);
                $('.price-value-' + productId).html(data.price);
                $('.price-value-' + productId).attr("data-ProductCartPrice", data.totalPrice);
                $('#hdnPlanProductId').val(selectedplanid);
                if (!isSimType) {
                    GetSimTypeAttributeByDeviceWithPlans(selectedplanid);
                }
            }
        });
    }

    function GetSimTypeAttributeByDeviceWithPlans(productId) {
        if (productId != null && productId != undefined && productId != "" && productId > 0) {
            $.ajax({
                cache: false,
                type: "POST",
                url: "/shoppingcart/GetSimTypeAttributeByDeviceWithPlan",
                data: { "productId": productId },
                dataType: 'json',
                success: function (response) {
                    $("#divSimType").html(response.html);
                },
                failure: function (response) {
                    alert(response);
                }
            });
        }
    }

    //#endregion


    //#region Plan With Product

    $('.planwithproductchange').on('change', function () {
        var selectedAttrName = $('input[type=radio].attr-monthlyprice:checked').data('attributename');
        $("#lblPaymentInstallments").html(selectedAttrName);
        var planproductid = $('#hdnPlanProductId').val();
        if (planproductid != null && planproductid != undefined && planproductid != "" && planproductid > 0) {
            $("#btnplanWithProduct_" + planproductid + "").trigger('click');
        }
    });

    productTemplate.fnProductDevice = function (mobilePrice, mobilePriceValue, selectedPlanId) {
        var selectedAttrValue = $('input[type=radio].attr-monthlyprice:checked').data('attributepricevalue');
        $('#hdnPlanProductId').val(selectedPlanId);
        $('#hdnSelectedAttributeValuePrice').val(selectedAttrValue);
        $('#simPrice').attr("data-simprice", mobilePriceValue);
        var productPrice = $('#productPrice').attr("data-productprice");
        fnAjaxCall(productPrice, mobilePriceValue, selectedAttrValue, true);
    }

    function fnAjaxCall(productPrice, planPrice, selectedAttrValue, isPlanWithDevice) {

        var adPayment = adControlPayment;
        var productId = $('#hdnProductId').val();
        $.ajax({
            url: '/shoppingcart/productdetails_addplan',
            data: 'productPrice=' + parseFloat(productPrice) + '&planPrice=' + parseFloat(planPrice) + '&advancePayment=' + adPayment + '&selectedAttributeValue=' + selectedAttrValue + '&isPlanWithDevice=' + isPlanWithDevice,
            type: 'post',
            success: function (data) {
                if (planPrice > 0) {
                    $('#trMobilePrice').show();
                    $('#simPrice').html(data.simPrice);
                }
                else {
                    $('#trMobilePrice').hide();
                }
                if (planPrice <= 0) {
                    $('#productPrice').html(data.deviceFormatedPrice + " <strong>/" + startFormMonth + "</strong>");
                    $('#spnProductFullPrice').html(data.formatedProductFullPrice);
                }
                $('#divTotalPrice').html(data.subTotalFormatedPrice);
                $('#spnTotalFullPrice').html(data.formatedTotalFullPrice);
                $('#spnSimFullPrice').html(data.formatedPlanFullPrice);
                $('.price-value-' + productId).html(data.price);
            }
        });
    }

    function fnLoadProductSimcard(productId) {
        resetSimCardSelection();
        $.ajax({
            url: '/shoppingcart/GetProductSimCards',
            data: { productId: productId },
            type: 'POST',
            success: function (response) {
                if (response != null) {
                    var simCardList = "<a onclick =\"SetSelectedSimCardValue(this, 0, 'Select')\" class=\"dropdown-item selected\">Select</a>";
                    for (var i = 0; i < response.simCardList.length; i++) {
                        simCardList += "<a onclick =\"SetSelectedSimCardValue(this, '" + response.simCardList[i].Value +"', '" + response.simCardList[i].Text+"')\" class=\"dropdown-item\">" + response.simCardList[i].Text + '</a>';
                    }
                    $('#ddlSimCardList1').html(simCardList);

                }
            }
        });
    }

    function resetSimCardSelection() {
        $("#simCardNumber").val(0);
        $("#dropdownSim").text('Select');
        $("#simValidation").hide();
        $(".dropdown-menu>a.selected").removeClass("selected");
    }

    //#endregion

}(window.productTemplate = window.productTemplate || {}, jQuery));