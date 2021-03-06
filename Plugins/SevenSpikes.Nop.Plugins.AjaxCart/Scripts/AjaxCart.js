!(function (u, c, l, s) {
    var r = ".ajaxCartInfo";
    function i() {
        return l.getAttrValFromDom(r, "data-productBoxProductItemElementSelector");
    }
    function p(t) {
        return l.getAttrValFromDom(t, "value") || t.text();
    }
    function a() {
        var t = l.getAttrValFromDom(r, "data-productPageAddToCartButtonSelector"),
            a = l.getAttrValFromDom(r, "data-productBoxAddToCartButtonSelector"),
            n = [],
            d = [];
        return (
            c(t).each(function () {
                if (!c(this).is(".nopAjaxCartProductVariantAddToCartButton")) {
                    var t = l.getAttrValFromDom(this, "data-productid", 0);
                    if (t && 0 < t) {
                        var a = { productId: t, isProductPage: !0, buttonElement: c(this) },
                            e = { productId: t, isProductPage: !0, buttonValue: p(c(this)) };
                        n.push(a), d.push(e);
                    }
                }
            }),
            c(a).each(function () {
                if (!c(this).is(".nopAjaxCartProductListAddToCartButton")) {
                    var t = ((o = c(this)), (r = i()) == s || "" === r ? 0 : l.getAttrValFromDom(c(o).parents(r), "data-productid", 0));
                    if (t && 0 < t) {
                        var a = { productId: t, isProductPage: !1, buttonElement: c(this) },
                            e = { productId: t, isProductPage: !1, buttonValue: p(c(this)) };
                        n.push(a), d.push(e);
                    }
                }
                var o, r;
            }),
            { addToCartButtons: n, buttonsParameters: d }
        );
    }
    function n() {
        var t = c("body");
        0 < t.length && c(".nopAjaxCartPanelAjaxBusy").height(t.height()).width(t.width()).show();
    }
    function e(t) {
        t
            ? (function () {
                if (0 < c(".miniProductDetailsView").length) {
                    var t = c(".miniProductDetailsView").parent().height(),
                        a = c(".miniProductDetailsView").parent().width();
                    c(".miniProductDetailsView .miniProductDetailsPanelAjaxBusy")
                        .height(t - 10)
                        .width(a - 10)
                        .show();
                }
            })()
            : n();
    }
    function m() {
        c(".nopAjaxCartPanelAjaxBusy").hide();
    }
    function g(t) {
        t ? c(".miniProductDetailsView .miniProductDetailsPanelAjaxBusy").hide() : m();
    }
    function o(t, a) {
        var e = c(t).filter("div[data-productId='" + a.productId + "']");
        if (0 < e.length) {
            var o = l.getAttrValFromDom(e, "data-isProductPage");
            o && o === a.isProductPage.toString() && (c(a.buttonElement).parents(".product-item, .add-to-cart").addClass("sevenspikes-ajaxcart"), c(a.buttonElement).replaceWith(e[0]));
        }
    }
    function d() {
        var e = a(),
            t = e.buttonsParameters;
        0 < t.length &&
            c.ajax({ cache: !1, type: "POST", data: c.toJSON(t), contentType: "application/json; charset=utf-8", url: l.getAttrValFromDom(r, "data-getAjaxCartButtonUrl") }).done(function (t) {
                if ("" !== t) {
                    for (var a = 0; a < e.addToCartButtons.length; a++) o(t, e.addToCartButtons[a]);
                    c.event.trigger({ type: "nopAjaxCartButtonsAddedEvent" });
                }
            });
    }
    function h() {
        var e = ".block.block-shoppingcart";
        0 < c(e).length &&
            c.ajax({ cache: !1, type: "GET", url: l.getHiddenValFromDom("#miniShoppingCartUrl") }).done(function (t) {
                var a = c(t).filter(e);
                c(e).replaceWith(a), c.event.trigger({ type: "nopAjaxCartMiniShoppingCartUpdated" });
            });
    }
    function v(t, packageId, productid) {

        t = '<div class="miniProductDetailsPanelAjaxBusy"></div><div class="clear"></div>' + t;
        //var existData = $("#addcart-responce-" + packageId).val();
        //t = t + existData;
        ajxSuccessResponce.push({
            "productid": productid,
            "responce": t
        });

        if (ajxPending.every(x => !x.pending)) {
            var responce = "";
            for (var pResponce = 0; pResponce < ajxSuccessResponce.length; pResponce++) {
                responce = responce + ajxSuccessResponce[pResponce].responce;
            }
            var a = c(".miniProductDetailsView").data("kendoWindow").content(responce);
            a.center(),
                a.open(),
                c(document).on("ajaxCart.product_attributes_changed", function (responce) {
                    if (responce.changedData && responce.changedData.pictureDefaultSizeUrl) {
                        var a = responce.changedData.pictureDefaultSizeUrl;
                        responce.element.closest(".product-overview-line").find(".picture img").attr("src", a);
                    }
                });
        }
        //else {
        //    $("#addcart-responce-" + packageId).val(t);
        //}

    }
    function f(t, o, a, packageid, ajxIndex) {
        var productid = t;
        var e = { productId: t, isAddToCartButton: a, packageid: packageid },
            r = l.getHiddenValFromDom("#getMiniProductDetailsViewUrl");
        c.ajax({ cache: !1, async: !1, type: "POST", data: e, url: r }).done(function (t) {
            if (packageid != 0) {
                ajxPending[ajxIndex].pending = false;
            }
            var a, e;
            v(t, packageid, productid),
                (a = o),
                0 < (e = l.getAttrValFromDom(".miniProductDetailsView .miniProductDetailsViewAddToCartButton, .miniProductDetailsView .nopAjaxCartProductVariantAddToWishlistButton", "data-productId", 0)) &&
                c(".miniProductDetailsView #addtocart_" + e + "_EnteredQuantity").val(a),
                c.event.trigger({ type: "nopAjaxCartMiniProductDetailsViewShown" });
        });
    }
    function C() {
        var e = l.getHiddenValFromDom("#flyoutShoppingCartPanelSelector");
        u.shouldRefreshFlyoutCart &&
            0 < c(e).length &&
            c.ajax({ cache: !1, type: "GET", url: l.getHiddenValFromDom("#flyoutShoppingCartUrl") }).done(function (t) {
                var a = c(t).filter(e);
                c(e).replaceWith(a), c.event.trigger({ type: "nopAjaxCartFlyoutShoppingCartUpdated" });
            });
    }
    function A() {
        var e = l.getHiddenValFromDom("#shoppingCartBoxPanelSelector");
        0 < c(e).length &&
            c.ajax({ cache: !1, type: "GET", url: l.getHiddenValFromDom("#shoppingCartBoxUrl") }).done(function (t) {
                var a = c(t).find(e);
                c(e).replaceWith(a), c.event.trigger({ type: "nopAjaxCartShoppingCartBoxUpdated" });
            });
    }
    function w(d) {
        var t = '.header-links a[href="/cart"]',
            a = l.getHiddenValFromDom("#shoppingCartMenuLinkSelector");
        0 < c("#shoppingCartMenuLinkSelector").length && "" !== a && ((t = a), (t = c("<div/>").html(t).text())),
            c(t).each(function (t, a) {
                var e = c(a).html(),
                    o = /\d+/.exec(e),
                    r = parseInt(o) + parseInt(d),
                    n = c(".ajaxCartInfo").attr("data-miniShoppingCartQuatityFormattingResource").replace("{0}", r);
                c(a).html(n);
            });
    }
    function P(d) {
        var t = "span.wishlist-qty",
            a = l.getHiddenValFromDom("#wishlistMenuLinkSelector");
        0 < c(a).length && (t = a),
            c(t).each(function (t, a) {
                var e = c(a).html(),
                    o = /\d+/.exec(e),
                    r = parseInt(o) + parseInt(d),
                    n = c(".ajaxCartInfo").attr("data-miniWishlistQuatityFormattingResource").replace("{0}", r);
                c(a).html(n);
            });
    }
    function x(t, packageid = 0, productid = 0) {

        if (packageid != 0) {
            //var existData = $("#addcart-responce-" + packageid).val();
            //t = t + existData;
            ajxSuccessResponce.push({
                "productid": productid,
                "responce": t
            });
            if (ajxPending.every(x => !x.pending)) {
                var responce = "";
                for (var pResponce = 0; pResponce < ajxSuccessResponce.length; pResponce++) {
                    responce = responce + ajxSuccessResponce[pResponce].responce;
                }
                //var html = c(t).html();
                c(".productAddedToCartWindow").html(responce);
                var a = c(".productAddedToCartWindow").data("kendoWindow");
                a.center(), a.open();
            }
            //else {
            //    $("#addcart-responce-" + packageid).val(t);
            //}
        }
        else {
            c(".productAddedToCartWindow").html(c(t).html());
            var a = c(".productAddedToCartWindow").data("kendoWindow");
            a.center(), a.open();
        }
    }
    function T(t, a) {
        var e = c(".addProductToCartErrors").data("kendoWindow");
        e ||
            (e = c(".addProductToCartErrors")
                .kendoWindow({ draggable: !1, resizable: !1, width: "300px", height: "100px", modal: !0, actions: ["Close"], animation: !1, visible: !1, title: a })
                .data("kendoWindow")).wrapper.addClass("ajaxCart"),
            e.content(t),
            e.center(),
            e.open(),
            c(document).on("click", ".k-overlay", function () {
                e.close();
            });
    }
    function j(t, a, e, o, r, n, packageid, ajxIndex) {

        var d = { productId: o, quantity: r, isAddToCartButton: e, packageId: packageid };
        return n
            ? (m(), void (u.waitForAjaxRequest = !1))
            : t.HasProductAttributes
                ? (f(o, r, e, packageid, ajxIndex), m(), void (u.waitForAjaxRequest = !1))
                : void c
                    .ajax({ cache: !1, type: "POST", data: d, url: l.getHiddenValFromDom("#addProductToCartUrl") })
                    .done(function (t) {
                        if (packageid != 0) {
                            ajxPending[ajxIndex].pending = false;
                        }
                        "success" === t.Status
                            ? e
                                ? (h(), C(), A(), w(r), x(t.ProductAddedToCartWindow, packageid, o), c.event.trigger({ type: "nopAjaxCartProductAddedToCartEvent", productId: o, quantity: r }))
                                : (P(r), x(t.ProductAddedToCartWindow, packageid, o), c.event.trigger({ type: "nopAjaxCartProductAddedToWishlistEvent", productId: o, quantity: r }))
                            : "warning" === t.Status
                                ? T(t.AddToCartWarnings, t.PopupTitle)
                                : "error" === t.Status && T(t.ErrorMessage, t.PopupTitle);
                    })
                    .always(function () {
                        m(), (u.waitForAjaxRequest = !1);
                    });
    }

    window.ajxPending = [];
    window.ajxSuccessResponce = [];
    function y(t, a) {
        if (!u.waitForAjaxRequest) {
            u.waitForAjaxRequest = !0;
            var packageId = Number(l.getAttrValFromDom(t.currentTarget, "data-packageId"));

            if (packageId != 0) {
                var productIds = $("#productId_" + packageId).val();
                var productIdList = productIds.split(",").map(Number);

                $("#addcart-responce-" + packageId).val('');
                ajxPending = [];
                ajxSuccessResponce = [];
                for (let i = 0; i < productIdList.length; i++) {
                    ajxPending.push({
                        "ajxIndex": i,
                        "pending": true
                    })
                    AddToCart(t, a, packageId, productIdList[i], i);
                }
            }
            else {
                productId = l.getAttrValFromDom(t.currentTarget, "data-productId") || c(t.currentTarget).parents(".product-item").attr("data-productid");
                AddToCart(t, a, 0, productId);
            }
        }
    }

    function AddToCart(t, a, packageId, productid, ajxIndex) {
        u.waitForAjaxRequest = !0;
        //var e = l.getAttrValFromDom(t.currentTarget, "data-productId") || c(t.currentTarget).parents(".product-item").attr("data-productid"),
        var e = productid,
            o = (function (t) {
                var a = 1,
                    e = l.getAttrValFromDom(t, "data-productId") || c(t).parents(".product-item").attr("data-productid") || "";
                if ("" !== e) {
                    var o = c(t)
                        .parents(".product-item")
                        .find("[data-quantityproductid=" + e + "]");
                    if (0 < o.length) {
                        var r = o.val();
                        0 < r && (a = r);
                    }
                }
                return a;
            })(t.currentTarget),
            r = { productId: e, quantity: o };
        n(),
            c
                .ajax({ cache: !1, type: "POST", data: r, url: l.getHiddenValFromDom("#checkProductAttributesUrl") })
                .done(function (t) {
                    j(t, 0, a, e, o, !1, packageId, ajxIndex);
                })
                .fail(function () {
                    j(data, 0, a, e, o, !0, packageId, ajxIndex);
                });
    }

    function D(o, r, n) {
        if ((r == s && (r = !1), !u.waitForAjaxRequest)) {
            u.waitForAjaxRequest = !0;
            var t = c(o.currentTarget),
                d = l.getAttrValFromDom(o.currentTarget, "data-productId") || c(o.currentTarget).parents(".product-item").attr("data-productid"),
                i = (function (t) {
                    var a = 1,
                        e = l.getAttrValFromDom(t, "data-productId");
                    if ("" !== e) {
                        var o = c("#addtocart_" + e + "_EnteredQuantity");
                        if (0 < o.length) {
                            var r = o.val();
                            0 < r && (a = r);
                        }
                    }
                    return a;
                })(t),
                a = t.closest("form").serialize();
            if (0 === a.length) {
                a = c("body").find("form").serialize();
            }
            0 !== (a = (a = a.replace(new RegExp("ajaxCart_product_attribute", "g"), "product_attribute")).replace(new RegExp("quickView_product_attribute", "g"), "product_attribute")).length && (a += "&"),
                (a += "productId=" + d),
                (a += "&isAddToCartButton=" + n),
                e(r),
                c
                    .ajax({ cache: !1, type: "POST", data: a, url: l.getHiddenValFromDom("#addProductVariantToCartUrl") })
                    .done(function (t) {
                        if ("warning" === t.Status || "error" === t.Status) {
                            var a = c(o.currentTarget).closest(".overview");
                            !(function (t, a, e, o) {
                                if (e) 0 < o.length ? o.find(".message-error").html(t) : c(".miniProductDetailsView .message-error").html(t);
                                else {
                                    var r = c(".addProductVariantToCartErrors").data("kendoWindow");
                                    r ||
                                        (r = c(".addProductVariantToCartErrors")
                                            .kendoWindow({ draggable: !1, resizable: !1, width: "300px", modal: !0, actions: ["Close"], animation: !1, visible: !1, title: a })
                                            .data("kendoWindow")).wrapper.addClass("ajaxCart"),
                                        r.content(t),
                                        r.center(),
                                        r.open(),
                                        c(document).on("click", ".k-overlay", function () {
                                            r.close();
                                        });
                                }
                            })(t.AddToCartWarnings, t.PopupTitle, r, a);
                        } else {
                            var packageId = Number($("#PackageId").val());
                            //var respionce = ajxSuccessResponce.find(p => p.productid == d);
                            //ajxSuccessResponce.splice(respionce, 1);
                            ajxSuccessResponce = $.grep(ajxSuccessResponce, function (e) {
                                return e.productid != d;
                            });
                            var e = "nopAjaxCartProductAddedToCartEvent";
                            n ? (h(), A(), w(i), C()) : (P(i), (e = "nopAjaxCartProductAddedToWishlistEvent")),
                                r && c(".miniProductDetailsView").data("kendoWindow").close(),
                                x(t.ProductAddedToCartWindow, packageId, Number(d)),
                                c.event.trigger({ type: e, productId: d, quantity: i });
                        }
                    })
                    .always(function () {
                        g(r), (u.waitForAjaxRequest = !1);
                    });
        }
    }
    (u.shouldRefreshFlyoutCart = !0),
        (u.waitForAjaxRequest = !1),
        (u.replaceAddToCartButtonsExternal = d),
        (u.closeProductAddedToCartWindow = function () {
            var t = c(".productAddedToCartWindow").data("kendoWindow");
            t && t.close();
        }),
        c(document).ready(function () {
            var t;
            d(),
                0 === (t = c("body")).find(".nopAjaxCartPanelAjaxBusy").length && (t.prepend('<div class="nopAjaxCartPanelAjaxBusy"></div>'), c(".nopAjaxCartPanelAjaxBusy").hide()),
                (function () {
                    var t = c("body");
                    if (0 < t.length) {
                        t.prepend('<div class="addProductToCartErrors"></div><div class="addProductVariantToCartErrors"></div><div class="miniProductDetailsView"></div><div class="productAddedToCartWindow"></div>');
                        var a = c(".miniProductDetailsView")
                            .kendoWindow({ draggable: !1, resizable: !1, modal: !0, actions: ["Close"], animation: !1, visible: !1 })
                            .data("kendoWindow");
                        a.wrapper.addClass("ajaxCart"),
                            c(document).on("click", ".k-overlay", function () {
                                a.close();
                            });
                    }
                    var e = c(".productAddedToCartWindow")
                        .kendoWindow({ draggable: !1, resizable: !1, modal: !0, actions: ["Close"], animation: !1, visible: !1 })
                        .data("kendoWindow");
                    e.wrapper.addClass("ajaxCart"),
                        c(document).on("click", ".k-overlay", function () {
                            e.close();
                        });
                })(),
                c("body").on("click", ".nopAjaxCartProductListAddToCartButton", function (t) {
                    t.preventDefault(), y(t, !0);
                }),
                c("body").on("click", ".nopAjaxCartProductVariantAddToCartButton", function (t) {
                    D(t, c(this).hasClass("miniProductDetailsViewAddToCartButton"), !0);
                }),
                c("body").on("click", ".miniProductDetailsViewAddToWishlistButton", function (t) {
                    D(t, !0, !1);
                }),
                c("body").on("click", ".productAddedToCartWindow .continueShoppingLink", function (t) {
                    t.preventDefault(), u.closeProductAddedToCartWindow();
                }),
                c(document).on("nopAjaxCartButtonsAddedEvent", function () {
                    var t, e, o, a;
                    (t = c(r)),
                        (e = "True" === t.attr("data-enableOnProductPage")),
                        (o = "True" === t.attr("data-enableOnCatalogPages")),
                        (a = t.attr("data-addToWishlistButtonSelector")),
                        c(a).each(function () {
                            var t = c(this),
                                a = i();
                            0 < t.parents(a).length
                                ? o &&
                                (t.prop("onclick", null).off("click"),
                                    t.on("click", function (t) {
                                        t.preventDefault(), y(t, !1);
                                    }))
                                : e &&
                                (t.prop("onclick", null).off("click"),
                                    t.on("click", function (t) {
                                        t.preventDefault(), D(t, !1, !1);
                                    }));
                        }),
                        (function () {
                            var t = c(".ajaxCartAllowedQuantitesHover");
                            if (0 !== t.length) {
                                var e = t.attr("data-productItemSelector");
                                c(".productQuantityDropdown")
                                    .off("mousedown")
                                    .on("mousedown", function () {
                                        var t = c(this).closest(e);
                                        t.addClass("ajax-cart-product-item-hover");
                                        var a = t.attr("data-productid");
                                        c(".productQuantityChanged" + a).val("yes");
                                    }),
                                    c(e)
                                        .off("mouseenter")
                                        .on("mouseenter", function () {
                                            var t = c(this).attr("data-productid"),
                                                a = c(".productQuantityChanged" + t);
                                            "yes" === a.val() && a.val("no");
                                        })
                                        .off("mouseleave")
                                        .on("mouseleave", function () {
                                            var t = c(this).attr("data-productid");
                                            "no" === c(".productQuantityChanged" + t).val() && c(this).removeClass("ajax-cart-product-item-hover");
                                        });
                            }
                        })();
                }),
                c(document).on("nopAjaxFiltersFiltrationCompleteEvent newProductsAddedToPageEvent", d),
                c(document).on("nopQuickViewDataShownEvent", d);
        });
})((window.nopAjaxCart = window.nopAjaxCart || {}), jQuery, sevenSpikesCore);
