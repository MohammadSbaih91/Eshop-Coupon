begin tran
update MessageTemplate set Body ='<p>  <a href="%Store.URL%"> %Store.Name%</a>  <br />  <br />  %EmailAFriend.FullName%  (%EmailAFriend.Email%) was shopping on %Store.Name% and wanted to share the following item with you.  <br />  <br />  <b><a target="_blank" href="%Product.ProductURLForCustomer%">%Product.Name%</a></b>  <br />  %Product.ShortDescription%  <br />  <br />  For more info click <a target="_blank" href="%Product.ProductURLForCustomer%">here</a>  <br />  <br />  <br />  %EmailAFriend.PersonalMessage%  <br />  <br />  %Store.Name%  </p>' where name ='Service.EmailAFriend'
select * from MessageTemplate  where name ='Service.EmailAFriend'
--commit
