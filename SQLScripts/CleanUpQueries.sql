

-- TNT.LON
--update	[EodPrices]
--	SET [Open] = [Open] * 100.0
--	,	[High] = [High] * 100.0
--	,	[Low] = [Low] * 100.0
--	,	[Close] = [Close] * 100.0
--	,	[AdjustedClose] = [AdjustedClose] * 100.0
--WHERE [ShareId] = 'A89EF42A-A29C-4780-DE5C-08D9EFDF465C' AND [Close] < 1.0

-- MTBCP
--DELETE FROM [EodPrices]
--	WHERE [Id] IN 
--	('9B9CBC2D-0EDF-4474-9CC0-53C0A33D3AFC'
--	,'F9878FA5-DDD1-4C03-BB10-DC1D936AC0A4'
--	,'E96D664F-BDAE-40E7-8A4F-B7926889C2B5'
--	,'0E4DA800-D7DE-45E5-9D18-CA774B150406'
--	,'8CA0128B-BE1B-43A3-A3E8-A20D5245844F'
--	,'902F085A-5D4D-454B-83CD-F12B016EAA52'
--	,'22EABE2E-4A79-414B-AA39-040E11C678A7')


select * 
	from IndexValues
	WHERE [IndexId] = 'AB27415D-6D08-48BA-8936-7A13E34D8ACA'
	ORDER BY [Day] DESC

select [ShareId], [Close], [AdjustedClose] 
	from EodPrices
	WHERE  [Day] = '2017-09-21'
	ORDER BY [AdjustedClose] DESC

-- Fix for Dodgy AdjustedClose values
UPDATE [EodPrices]
	SET [AdjustedClose] = [Close]
	WHERE (ABS([AdjustedClose] - [Close])/[Close]) > 0.5
