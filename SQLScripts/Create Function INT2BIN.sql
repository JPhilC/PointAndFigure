CREATE FUNCTION INT2BIN
(
 @value INT,
 @fixedSize INT = 10
)
RETURNS VARCHAR(1000)
AS
BEGIN
 DECLARE @result VARCHAR(1000) = '';

 WHILE (@value != 0)
 BEGIN
  IF(@value%2 = 0) 
   SET @Result = '0' + @Result;
  ELSE
   SET @Result = '1' + @Result;
   
  SET @value = @value / 2;
 END;

 IF(@fixedSize IS NOT NULL AND @fixedSize > 0 AND LEN(@Result) < @fixedSize)
 BEGIN
  DECLARE @len INT = @fixedSize;
  DECLARE @padding VARCHAR(1000) = '';
 
  WHILE @len > 0
  BEGIN
   SET @padding = @padding + '0';
   SET @len = @len-1;
  END; 
  SET @result = RIGHT(@padding + @result, @fixedSize);
 END;
 
 RETURN @result;
END
GO