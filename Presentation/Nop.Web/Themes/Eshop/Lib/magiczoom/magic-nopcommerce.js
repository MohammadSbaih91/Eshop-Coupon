/*

   Utility script to integrate Magic Toolbox tools with nopCommerce
   Copyright 2017 Magic Toolbox
   https://www.magictoolbox.com/nopcommerce/

*/
function MagicToolbox_addCSSRule(selector, declaration) {
    var ua = navigator.userAgent.toLowerCase();

    var isIE = (/msie/.test(ua)) && !(/opera/.test(ua)) && (/win/.test(ua)) && (!/msie 9\.0/.test(ua)) && (!/msie 10\.0/.test(ua));

    var style_node = document.createElement("style");
    style_node.setAttribute("type", "text/css");
    style_node.setAttribute("media", "screen");

    if (!isIE) style_node.appendChild(document.createTextNode(selector + " {" + declaration + "}"));

    document.getElementsByTagName("head")[0].appendChild(style_node);

    if (isIE && document.styleSheets && document.styleSheets.length > 0) {
        var last_style_node = document.styleSheets[document.styleSheets.length - 1];
        if (typeof(last_style_node.addRule) == "object") last_style_node.addRule(selector, declaration);
    }
}

function MagicToolbox_getLargeImage(src) {
    return src.replace(/^(.*)_[0-9]{1,}(\.[a-z]{1,})$/gm,"$1$2");
}

$mjs(document)[(typeof $mjs(document).jAddEvent)=='function'?'jAddEvent':'je1']('domready', function() {

    var MagicToolbox_mainImage = true,
        MagicToolbox_firstSelector = true,
        MagicToolbox_thumbSize,

        $MAGICJS = (typeof magicJS === 'undefined') ? $J : magicJS;


    if ('MagicZoomPlus' === MagicToolbox_toolName) {
        MagicToolbox_toolName = 'MagicZoom';
    }

    $mjs( $MAGICJS.$A($mjs(document).byTag('div')).filter(function(o){
        if (o.className=='gallery') {
            return true;
        }
    }) ).forEach(function(o) {
        $mjs( $MAGICJS.$A($mjs(o).byTag('img')) ).forEach(function(o) {
            if (MagicToolbox_mainImage) {
                var ael = document.createElement('A');
                ael.href = MagicToolbox_getLargeImage(o.src);
                MagicToolbox_thumbSize = parseInt(o.src.replace(/^.*_([0-9]{1,})\.[a-z]{1,}$/gm,"$1"));
                MagicToolbox_addCSSRule('.'+MagicToolbox_toolName+' img ', 'max-width:'+MagicToolbox_thumbSize+'px');
                MagicToolbox_addCSSRule('.gallery .picture:before', 'content:none;');
                MagicToolbox_addCSSRule('.gallery .picture img, .gallery .picture-thumbs img, .variant-picture img', 'position:static;');
                ael.className = MagicToolbox_toolName;
                ael.setAttribute('title', o.getAttribute('alt'));
                ael.setAttribute('id', 'MagicImage');
                var iel = document.createElement('IMG');
                iel.setAttribute('src', o.src);
                iel.setAttribute('alt', o.getAttribute('alt'));
                ael.appendChild(iel);
                o.parentNode.replaceChild(ael,o);
                MagicToolbox_mainImage = false;
            } else {
                var ael = document.createElement('A');
                ael.href = o.getAttribute('data-fullsize');                
                ael.setAttribute('data-image', o.getAttribute('data-defaultsize'));
                ael.setAttribute('class','MagicSelector');
                var $img = o.cloneNode();
                ael.appendChild($img);
                if ( 'MagicZoom' == MagicToolbox_toolName ) {
                    ael.setAttribute('data-zoom-id', 'MagicImage');
                } else if ( 'MagicThumb' == MagicToolbox_toolName ) {
                    ael.setAttribute('data-thumb-id', 'MagicImage');
                }
                o.parentNode.replaceChild(ael,o);
            }
        });
    });
    switch(MagicToolbox_toolName) {
        case 'MagicZoom': MagicZoom.start(); break;
        case 'MagicThumb': MagicThumb.start(); break;
    }

    mtInitSirv();
});

function SirvToggleMT(zoom, spin, elm) {
    if(zoom) {
        jQuery('#SirvContainer').hide();
        jQuery('#ImageContainer').show();
    } else {
        jQuery('#SirvContainer').show();
        jQuery('#ImageContainer').hide();
    }

    jQuery('.active-magic-selector').removeClass('active-magic-selector');
    jQuery(elm).addClass('active-magic-selector');

    return false;
}

function mtInitSirv() {

    if (typeof(SirvID) == 'undefined' || SirvID == '') {
        return;
    }

    if (typeof SirvIconURL == 'undefined') {
        var SirvIconURL = 'https://magictoolbox.sirv.com/images/misc/360icon.jpg';
    }

    var sirv = document.createElement('script');
    sirv.type = 'text/javascript';
    sirv.async = true;
    sirv.src = document.location.protocol.replace('file:', 'http:') + '//scripts.sirv.com/sirv.js';
    document.getElementsByTagName('script')[0].parentNode.appendChild(sirv);

    var SirvProductID = jQuery('div[data-productid]').data('productid');

    var spinURL = document.location.protocol.replace('file:', 'http:') + '//' + SirvID + '.sirv.com/' + SirvSpinsPath.replace(/{product\-id}/g, SirvProductID);

    jQuery.ajax({
        url: spinURL,
        dataType: 'jsonp',
        cache: true,
        timeout : 4000,
        error : function(jqXHR, textStatus, errorThrown) {            
        },
        success: function(data, textStatus, jqXHR) {
            jQuery('#MagicImage').after('<div class="MTGallery"><div id="ImageContainer"><div></div></div><div style="display:none;" id="SirvContainer"></div></div>');
            jQuery('#MagicImage').appendTo('#ImageContainer div');
            jQuery('#SirvContainer').append('<div class="Sirv" id="sirv-spin" data-src="'+spinURL+'"></div>');
            jQuery('a[data-zoom-id],a[data-thumb-id]').on('click touchend',function(e) {
                SirvToggleMT(true, false, this);
                e.preventDefault();
            });
            jQuery('.thumb-item:last').after('<div class="thumb-item"><a class="MagicSelector" href="#"><img id="SirvIcon" style="display:none;" src="'+SirvIconURL+'"/></a></div>');
            jQuery('.thumb-item:first img').one('load', function() {
                jQuery('#SirvIcon').attr('height', jQuery(this).height()+'px').show();
            }).each(function() {
                if(this.complete) $(this).load();
            });            
            jQuery('#SirvIcon').closest('a').on('click touchend',function(e) {
                SirvToggleMT(false, true, this);
                e.preventDefault();
            });
            jQuery('body').append('<style type="text/css">.mz-thumb:hover:not(.mz-thumb-selected) img {-webkit-filter: none;filter: none;}.mz-thumb-selected img {-webkit-filter: none;filter: none;}.no-cssfilters-magic .mz-thumb {background: none;}.no-cssfilters-magic .mz-thumb:hover:not(.mz-thumb-selected) img {opacity: 1;filter: none;}.no-cssfilters-magic .mz-thumb-selected img {opacity: 1;filter: none;}.active-magic-selector img {-webkit-filter: brightness(60%);filter: brightness(60%);}.no-cssfilters-magic .mz-thumb:hover:not(.active-magic-selector) img {opacity: .75;filter: alpha(opacity=75);}.no-cssfilters-magic .active-magic-selector img {opacity: .6;filter: alpha(opacity=60);}</style>');
            jQuery('.thumb-item:first a').addClass('active-magic-selector')
        },
    });
}

window['mgctlbx$Pltm'] = 'nopCommerce';

if (typeof($.fn.slimbox)!='undefined') $.fn.slimbox = function() {};
if (typeof($.fn.magnificPopup)!='undefined') $.fn.magnificPopup = function() {};
