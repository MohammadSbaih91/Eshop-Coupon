begin tran
update Setting set Value ='ExcludingTax' where name ='taxsettings.taxdisplaytype'
update Setting set Value ='False' where name ='taxsettings.pricesincludetax'
update Setting set Value ='False' where name ='taxsettings.displaytaxrates'
update Setting set Value ='False' where name ='taxsettings.allowcustomerstoselecttaxdisplaytype'
update Setting set Value ='False' where name ='taxsettings.displaytaxsuffix'

update Setting set Value ='True' where name ='paymentsettings.bypasspaymentmethodselectionifonlyone'
update Setting set Value ='True' where name ='shippingsettings.bypassshippingmethodselectionifonlyone'

update Setting set Value ='True' where name ='catalogsettings.allowanonymoususerstoemailafriend'
update Setting set Value ='True' where name ='ordersettings.anonymouscheckoutallowed'

update Setting set Value ='True' where name ='EShopSettings.HidePaymentFromOpc'
update Setting set Value ='True' where name ='EShopSettings.HideShippingMethodFromOpc'

select * from setting
--select * from setting 
--commit
