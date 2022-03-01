/*
** nopCommerce one page checkout
*/


var Checkout = {
    loadWaiting: false,
    failureUrl: false,

    init: function (failureUrl) {
        this.loadWaiting = false;
        this.failureUrl = failureUrl;

        Accordion.disallowAccessToNextSections = true;
    },

    ajaxFailure: function () {
        location.href = Checkout.failureUrl;
    },

    _disableEnableAll: function (element, isDisabled) {
        var descendants = element.find('*');
        $(descendants).each(function () {
            if (isDisabled) {
                $(this).prop("disabled", true);
            } else {
                $(this).prop("disabled", false);
            }
        });

        if (isDisabled) {
            element.prop("disabled", true);
        } else {
            $(this).prop("disabled", false);
        }
    },

    setLoadWaiting: function (step, keepDisabled) {
        if (step) {
            $("#divAjaxLoader").addClass("active");

            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            var container = $('#' + step + '-buttons-container');
            container.addClass('disabled');
            container.css('opacity', '.5');
            this._disableEnableAll(container, true);
            $('#' + step + '-please-wait').show();
        } else {
            $("#divAjaxLoader").removeClass("active");
            if (this.loadWaiting) {
                var container = $('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = (keepDisabled ? true : false);
                if (!isDisabled) {
                    container.removeClass('disabled');
                    container.css('opacity', '1');
                }
                this._disableEnableAll(container, isDisabled);
                $('#' + this.loadWaiting + '-please-wait').hide();
            }
        }
        this.loadWaiting = step;
    },

    gotoSection: function (section) {
        $("#checkout-" + section).addClass("active");
        section = $('#opc-' + section);
        section.addClass('allow');
        Accordion.openSection(section);
    },

    back: function () {
        if (this.loadWaiting) return;
        Accordion.openPrevSection(true, true);
    },

    setStepResponse: function (response) {
        if (response.update_section) {
            $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
            //if (response.update_section.name == "shipping") {
            //    $(document).ready(function () {
            //        var showLinkToResultSearch;
            //        var searchText;
            //        $('#search-city').autocomplete({
            //            delay: 500,
            //            minLength: 3,
            //            source: '/AppointmentBooking/SearchCity',
            //            appendTo: '.search-city',
            //            select: function (event, ui) {
            //                $("#search-city").val(ui.item.name);
            //                return false;
            //            },
            //            select: function (event, ui) {
            //                $("#search-city").val(ui.item.name);
            //                $("#CityId").val(ui.item.id);
            //                $("#BranchId").html(ui.item.regionID);

            //                return false;
            //            },
            //            //append link to the end of list
            //            open: function (event, ui) {
            //                //display link to search page
            //                if (showLinkToResultSearch) {
            //                    searchText = document.getElementById("search-city").value;
            //                    $(".ui-autocomplete").append("<li class=\"ui-menu-item\" role=\"presentation\"><a href=\"/search?q=" + searchText + "\"></a></li>");
            //                }
            //            }
            //        })
            //            .data("ui-autocomplete")._renderItem = function (ul, item) {
            //                var t = item.name;
            //                //html encode
            //                t = htmlEncode(t);
            //                return $("<li></li>")
            //                    .data("item.autocomplete", item)
            //                    .append("<a><span>" + t + "</span></a>")
            //                    .appendTo(ul);
            //            };
            //    });
            //}

            $('html, body').animate({
                scrollTop: $('#checkout-' + response.update_section.name + '-load').offset().top - 110
            }, 0);
        }
        if (response.allow_sections) {
            response.allow_sections.each(function (e) {
                $('#opc-' + e).addClass('allow');
            });
        }

        //TODO move it to a new method
        if ($("#billing-address-select").length > 0) {
            Billing.newAddress(!$('#billing-address-select').val());
        }
        if ($("#shipping-address-select").length > 0) {
            Shipping.newAddress(!$('#shipping-address-select').val());
        }

        if (response.goto_section) {
            Checkout.gotoSection(response.goto_section);
            return true;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    }
};





var Billing = {
    form: false,
    saveUrl: false,
    disableBillingAddressCheckoutStep: false,
    isSuccess: false,

    init: function (form, saveUrl, disableBillingAddressCheckoutStep,successUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.disableBillingAddressCheckoutStep = disableBillingAddressCheckoutStep;
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            $('#billing-new-address-form').show();
        } else {
            $('#billing-new-address-form').hide();
        }
        $(document).trigger({ type: "onepagecheckout_billing_address_new" });
    },

    resetSelectedAddress: function () {
        var selectElement = $('#billing-address-select');
        if (selectElement) {
            selectElement.val('');
        }
        $(document).trigger({ type: "onepagecheckout_billing_address_reset" });
    },

    save: function () {
        displayAjaxLoading(true);
        if (Checkout.loadWaiting != false) return;
        var radioValue = $('input[name=PickUpInStore]:checked').val();
        if (radioValue == "true") {
            OpenDrawerBookAppointment();
            displayAjaxLoading(false);
            return true;
        }
        else {
            Checkout.setLoadWaiting('billing');

            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    saveCustomBilling: function () {
        displayAjaxLoading(true);
        if (Checkout.loadWaiting != false) return;
        Checkout.setLoadWaiting('billing');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        //ensure that response.wrong_billing_address is set
        //if not set, "true" is the default value
        if (typeof response.wrong_billing_address == 'undefined') {
            response.wrong_billing_address = false;
        }
        if (Billing.disableBillingAddressCheckoutStep) {
            if (response.wrong_billing_address) {
                Accordion.showSection('#opc-billing');
            } else {
                Accordion.hideSection('#opc-billing');
            }
        }


        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }
        
        if (response.success) {
            if (response.redirect) {
                Shipping.isSuccess = true;
                location.href = response.redirect;
                return;
            }
            if (response.success) {
                Shipping.isSuccess = true;
                window.location = Shipping.successUrl + "/" + response.orderId;
            }
        }
        else {
            Checkout.setStepResponse(response);
        }
    }
};



var Shipping = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (form, saveUrl, successUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            $('#shipping-new-address-form').show();
        } else {
            $('#shipping-new-address-form').hide();
        }
        $(document).trigger({ type: "onepagecheckout_shipping_address_new" });
    },

    togglePickUpInStore: function (pickupInStoreInput) {
        if (pickupInStoreInput.checked) {
            $('#pickup-points-form').hide();
            $('#shipping-addresses-form').hide();
            $("#opc-payment_method").hide();
            $("#opc-payment_info").hide();
            $("#orderconfirmno").text("3");
        }
        else {
            $('#pickup-points-form').hide();
            $('#shipping-addresses-form').show();
            $("#opc-payment_method").show();
            $("#opc-payment_info").show();
            $("#orderconfirmno").text("5");
        }
    },

    resetSelectedAddress: function () {
        var selectElement = $('#shipping-address-select');
        if (selectElement) {
            selectElement.val('');
        }
        $(document).trigger({ type: "onepagecheckout_shipping_address_reset" });
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('shipping');

        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }
        if (response.success) {
            if (response.redirect) {
                Shipping.isSuccess = true;
                location.href = response.redirect;
                return;
            }
            if (response.success) {
                Shipping.isSuccess = true;
                window.location = Shipping.successUrl + "/" + response.orderId;
            }
        }
        else {
            Checkout.setStepResponse(response);
        }
    }
};



var ShippingMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    validate: function () {
        var methods = document.getElementsByName('shippingoption');
        if (methods.length == 0) {
            alert('Your order cannot be completed at this time as there is no shipping methods available for it. Please make necessary changes in your shipping address.');
            return false;
        }

        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify shipping method.');
        return false;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        if (this.validate()) {
            Checkout.setLoadWaiting('shipping-method');

            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentMethod = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (form, saveUrl, successUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    toggleUseRewardPoints: function (useRewardPointsInput) {
        if (useRewardPointsInput.checked) {
            $('#payment-method-block').hide();
        }
        else {
            $('#payment-method-block').show();
        }
    },

    validate: function () {
        var methods = document.getElementsByName('paymentmethod');
        if (methods.length == 0) {
            alert('Your order cannot be completed at this time as there is no payment methods available for it.');
            return false;
        }

        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify payment method.');
        return false;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        if (this.validate()) {
            Checkout.setLoadWaiting('payment-method');
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        if (response.redirect) {
            PaymentMethod.isSuccess = true;
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            PaymentMethod.isSuccess = true;
            window.location = PaymentMethod.successUrl + "/" + response.orderId;
        }

        //Checkout.setStepResponse(response);
    }
};



var PaymentInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('payment-info');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var ConfirmOrder = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) {
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        //terms of service
        var termOfServiceOk = true;
        if ($('#termsofservice').length > 0) {
            //terms of service element exists
            if (!$('#termsofservice').is(':checked')) {
                $("#terms-of-service-warning-box").dialog();
                termOfServiceOk = false;
            } else {
                termOfServiceOk = true;
            }
        }
        if (termOfServiceOk) {
            Checkout.setLoadWaiting('confirm-order');
            $.ajax({
                cache: false,
                url: this.saveUrl,
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        } else {
            return false;
        }
    },

    resetLoadWaiting: function (transport) {
        Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        if (response.redirect) {
            ConfirmOrder.isSuccess = true;
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            ConfirmOrder.isSuccess = true;
            window.location = ConfirmOrder.successUrl + "/" + response.orderId;
        }

        Checkout.setStepResponse(response);
    }
};