CREATE TABLE [AppointmentBranch] (
    [Id] int NOT NULL IDENTITY,
    [BranchId] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [Longitude] nvarchar(max) NULL,
    [Latitude] nvarchar(max) NULL,
    CONSTRAINT [PK_AppointmentBranch] PRIMARY KEY ([Id])
)
GO

INSERT [dbo].[MessageTemplate] ([Name], [BccEmailAddresses], [Subject], [Body], [IsActive], [DelayBeforeSend], [DelayPeriodId], [AttachedDownloadId], [EmailAccountId], [LimitedToStores]) VALUES (N'OrderPlaced.AppointmentBooked', NULL, N'Pickup appointment booked', N'<style>
.order-confirm-page { }
.order-confirm-page .appoint-booked { }
.order-confirm-page .appoint-booked .info-desc { margin: 0 40px 40px 0; }
.order-confirm-page .appoint-booked .info-desc h2 { font-size: 34px; margin: 0 0 30px !important; line-height: 45px; }
.order-confirm-page .appoint-booked .info-desc h2 span { color: #F16E00; }
.order-confirm-page .appoint-booked .info-desc p { font-size: 14px; color: #313131; font-weight: 600; }
.order-confirm-page .appoint-booked .info-desc p span { color: #F16E00; }
.order-confirm-page .appoint-booked .booked-bootom { display: flex; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket { margin: 0 20px; min-width: 142px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket { position: relative; text-align: center; background: #000; padding: 10px; color: #fff; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket ._cir { position: absolute; background: #fff; height: 15px; width: 15px; border-radius: 180px !important; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .c_cir { top: 10px; position: absolute; background: #fff; height: 20px; width: 20px; border-radius: 180px !important; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .c_cir.cL { left: -20px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .c_cir.cR { right: -20px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket ._cir.topl { left: -7px; top: -7px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket ._cir.topr { right: -7px; top: -7px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket ._cir.topbl { left: -7px; bottom: -7px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket ._cir.topbr { right: -7px; bottom: -7px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .top { border-bottom: 2px dashed #797777; padding: 0 0 15px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .top .org-layer { background: #F16E00; text-align: center; display: inline-block; padding: 33px 6px 6px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .top h4 { color: #fefefe; font-size: 13px; padding: 10px 0 15px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .top h2 { color: #fff; font-size: 26px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .bottom { position: relative; margin: 0 0 20px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-ticket .ticket .bottom p { color: #cecece; font-size: 14px; margin-top: 10px; }
.order-confirm-page .appoint-booked .booked-bootom ._flex_data { display: flex; }
.order-confirm-page .appoint-booked .booked-bootom .booked-img { margin-top: 97px; }
.order-confirm-page .appoint-booked .booked-bootom .booked-img img { max-width: 100%; }
</style>
<div class="order-confirm-page">
    <div class="appoint-booked">
        <div class="info-desc">
            <h2><span>Order</span> Confirmed and <span>the Pickup</span> appointment is booked</h2>
            <p>
                Your order is confirmed and the number is <span>%Store.Id%</span>. Orange will send you an email containing the appointment details. And for you to see the order details you can check and print the order summary.
            </p>
        </div>
        <div class="booked-bootom">
            <div class="_flex_data">
                <div class="booked-ticket">
                    <div class="ticket">
                        <div class="_cir topl"></div><div class="_cir topr"></div>
                        <div class="top">
                            <div class="org-layer">Orange</div>
                            <h4>Appointment Ticket</h4>
                            <h2>%Store.TicketNumber%</h2>
                        </div>
                        <div class="bottom">
                            <div class="c_cir cL"></div><div class="c_cir cR"></div>

                            <p>%Store.AppointmentDate%,<br>%Store.AppointmentTime%</p>
                        </div>
                        <div class="_cir topbl"></div><div class="_cir topbr"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>', 1, NULL, 0, 0, 1, 0)
GO
INSERT INTO AppointmentBranch VALUES('1365.0','Boulevard shop','35.905087','31.964272')
INSERT INTO AppointmentBranch VALUES('561.0','Franchise Abdali','35.909203','31.969954')
INSERT INTO AppointmentBranch VALUES('592.0','Franchise - Jabal Alhussein','35.91473','31.969532')
INSERT INTO AppointmentBranch VALUES('1509.0','Franchise - Safeway Shmaisani','35.89459','31.981894')
INSERT INTO AppointmentBranch VALUES('1485.0','Franchise - Istqlal Mall','35.922115','31.97842')
INSERT INTO AppointmentBranch VALUES('466.0','Franchise Al Khayyam','35.92987','31.952814')
INSERT INTO AppointmentBranch VALUES('1246.0','Franchise Smartbuy University street','35.892323','31.990734')
INSERT INTO AppointmentBranch VALUES('1525.0','Franchise Smartbuy Taj Mall','35.88799','31.941141')
INSERT INTO AppointmentBranch VALUES('1262.0','Flagship Shop','35.882214','31.941502')
INSERT INTO AppointmentBranch VALUES('1144.0','Tla'' Al-Ali Center','35.8702','31.9782')
INSERT INTO AppointmentBranch VALUES('702.0','Franchise Nazal Shop','35.9134','31.932714')
INSERT INTO AppointmentBranch VALUES('584.0','Integrated Gardens','35.8753','31.9886')
INSERT INTO AppointmentBranch VALUES('512.0','Al-Ashrafieh Center','35.9382','31.9397')
INSERT INTO AppointmentBranch VALUES('1216.0','Franchise - Safeway 7Th Circle','35.85739','31.95777')
INSERT INTO AppointmentBranch VALUES('834.0','Sweifieh Center','35.8568','31.9557')
INSERT INTO AppointmentBranch VALUES('1504.0','Franchise Murjan Mall','35.873074','31.996857')
INSERT INTO AppointmentBranch VALUES('1325.0','Franchise Smartbuy 7th circle','35.855263','31.951715')
INSERT INTO AppointmentBranch VALUES('797.0','Franchise – Hashmi Shamali','35.95483','31.97608')
GO

Update StorePickupPoint SET DisplayOrder = 99
GO

INSERT INTO StorePickupPoint VALUES('As Appointment booking ticket','admin@yourstore.com',0,0,null,0,0)
GO