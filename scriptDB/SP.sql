﻿use qlmuahang
go
-- Lấy tất cả YCBG
create proc sp_get_ycbg
	@maYCBG varchar(10)
as
begin
	if(@maYCBG <> '')
		select * from YeuCauBaoGia where MaYCBaoGia like '%'+@maYCBG+'%'
	else
	select * from YeuCauBaoGia
end
go
-- Lấy tất cả Mã NCC có sản phẩm cung cấp
create proc sp_get_all_maNCC
	@masp int
as
begin
	if not @masp is null
		select MaNCC from CTSP where MaSP = @masp
	else
		select MaNCC from CTSP
		group by MaNCC
end
go
-- lấy mã sp và tên sản phẩm mà tồn tại trong CTSP hoặc do NCC nào đó cung cấp
go
create proc sp_get_all_masp
	@mancc varchar(10)
as
begin
	if @mancc <> ''
		select MaSP, TenSanPham
		from SanPham
		where MaSP in (select MaSP from CTSP where MaNCC = @mancc)
	else
		select MaSP, TenSanPham
		from SanPham
		where MaSP in (select MaSP from CTSP)
end
go
-- declare type to input from WPF to SQL
create type CTYCBGType as table (
	MaNCC varchar(max),
	MaSP int,
	SLSeMua int
)
go
-- Thêm từng dòng trong CTYCBG gửi từ WPF
create proc sp_add_CTYCBG
	@maYCBG varchar(10),
	@ctycbg CTYCBGType readonly
as
begin
	-- declare
	declare cr_CTYCBG cursor forward_only
	for select * from @ctycbg
	declare @mancc varchar(10), @masp varchar(10), @sl int
	open cr_CTYCBG
	fetch next from cr_CTYCBG into @mancc, @masp, @sl
	-- tran
	set xact_abort on
	begin tran
		while @@FETCH_STATUS = 0
		begin
			insert into CTYCBaoGia(MaNCC, MaSP, SLSeMua, MaYCBaoGia)
			values (@mancc, @masp, @sl, @maYCBG)

			fetch next from cr_CTYCBG into @mancc, @masp, @sl
		end
	commit tran
	close cr_CTYCBG
	deallocate cr_CTYCBG
end
go
-- Tạo YCBG và CTYCBG
create proc sp_add_YCBG
	@ctYCBG CTYCBGType readonly
as
begin
	-- find MaYCBH valid
	declare @maYCBG int
	select @maYCBG = count(MaYCBaoGia) from YeuCauBaoGia
	if exists (select * from YeuCauBaoGia with (updlock) where MaYCBaoGia = @maYCBG+'')
		select @maYCBG = @maYCBG + 1
	-- create
	set xact_abort on
	begin tran
		declare @ma varchar(10), @error int
		set @ma = (select cast(@maYCBG as varchar(10)))
		-- create YCBG
		select  * from YeuCauBaoGia
		insert into YeuCauBaoGia
		values (
			@ma,
			GETDATE(), 
			NULL, 
			(select manv from Account
				where tendangnhap in (select ORIGINAL_LOGIN()))
		)
		-- create CTYCBH
		exec @error = sp_add_CTYCBG @ma, @ctYCBG
	commit tran
	if @@ERROR <> 0 or @error <> 0
	begin
		ROLLBACK
		DECLARE @ErrorMessage VARCHAR(2000)
		SELECT @ErrorMessage = 'Lỗi: ' + ERROR_MESSAGE()
		RAISERROR(@ErrorMessage, 16, 1)
	end
end
go
-- Lấy tát cả CTYCBG của YCBG
create proc sp_get_CTYCBG
	@maYCBG varchar(10)
as
begin
	select * 
	from CTYCBaoGia A, (select MaSP, TenSanPham from SanPham) as B
	where MaYCBaoGia = @maYCBG
		and A.MaSP = B.MaSP
end
go
--
create proc sp_get_tenSP 
	@maSP int
as
begin
	select TenSanPham from SanPham where MaSP = @maSP
end
