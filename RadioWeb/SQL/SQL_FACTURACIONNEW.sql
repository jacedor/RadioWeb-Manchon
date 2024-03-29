SET TERM ^ ;
CREATE PROCEDURE FACTURACION_NEW (
    FECHAINICIAL TIMESTAMP,
    FECHAFINAL TIMESTAMP,
    ANYS INTEGER,
    MUTUA VARCHAR(100),
    CENTRO INTEGER,
    PAGADO VARCHAR(1),
    GPR INTEGER,
    COLEGIADO INTEGER,
    MEDICO INTEGER,
    FACTURADO VARCHAR(1),
    TIPOEXPLORACION INTEGER,
    TECNICO INTEGER )
RETURNS (
    ORDEN INTEGER,
    COD_GRUP VARCHAR(10),
    SANTERIOR NUMERIC(9,2),
    SANTERIORCONSUM NUMERIC(9,2),
    SANTERIORTOTAL NUMERIC(9,2),
    SACTUAL NUMERIC(9,2),
    SACTUALCONSUM NUMERIC(9,2),
    SACTUALTOTAL NUMERIC(9,2),
    DIFCANTIDAD NUMERIC(9,2),
    DIFCANTIDADPORC NUMERIC(9,2),
    CANTERIOR INTEGER,
    CACTUAL INTEGER,
    DIFCUENTA INTEGER,
    DIFCUENTAPORC NUMERIC(9,2) )
AS
DECLARE VARIABLE ANYACTUAL INTEGER;
  DECLARE VARIABLE ANYPASSAT INTEGER;
  DECLARE VARIABLE FINICIALPASSAT TIMESTAMP;
  DECLARE VARIABLE FFINALPASSAT TIMESTAMP;
  DECLARE VARIABLE pCENTROMAX INTEGER;
  DECLARE VARIABLE pCENTROMIN INTEGER;
  DECLARE VARIABLE pMUTUAMAX INTEGER;
  DECLARE VARIABLE pMUTUAMIN INTEGER;
  DECLARE VARIABLE pPAGADO VARCHAR(1);
  DECLARE VARIABLE pFACTURADO VARCHAR(1);
  DECLARE VARIABLE pGPRMIN INTEGER; 
  DECLARE VARIABLE pGPRMAX INTEGER;
  DECLARE VARIABLE pCOLEGIADOMAX INTEGER;
  DECLARE VARIABLE pCOLEGIADOMIN INTEGER;
  DECLARE VARIABLE pMEDICOMAX INTEGER;
  DECLARE VARIABLE pMEDICOMIN INTEGER;
  DECLARE VARIABLE MUTUA0 INTEGER;
  DECLARE VARIABLE MUTUA1 INTEGER;
  DECLARE VARIABLE MUTUA2 INTEGER;
  DECLARE VARIABLE MUTUA3 INTEGER;
  DECLARE VARIABLE MUTUA4 INTEGER;
  DECLARE VARIABLE MUTUA5 INTEGER;
  DECLARE VARIABLE MUTUA6 INTEGER;
  DECLARE VARIABLE MUTUA7 INTEGER;
  DECLARE VARIABLE MUTUA8 INTEGER;
  DECLARE VARIABLE MUTUA9 INTEGER;
  DECLARE VARIABLE pIOR_TIPOEXPLORACIONMIN INTEGER;
  DECLARE VARIABLE pIOR_TIPOEXPLORACIONMAX INTEGER;
  DECLARE VARIABLE pIOR_TECNICOMIN INTEGER;
  DECLARE VARIABLE pIOR_TECNICOMAX INTEGER;
  
BEGIN

  ANYACTUAL=EXTRACT(YEAR FROM :FECHAFINAL);
  ANYPASSAT=ANYACTUAL-ANYS;
  FINICIALPASSAT=CAST(REPLACE(FECHAINICIAL,ANYACTUAL,ANYPASSAT) AS TIMESTAMP);
  FFINALPASSAT=CAST(REPLACE(FECHAFINAL,ANYACTUAL,ANYPASSAT) AS TIMESTAMP);

 MUTUA0=0;MUTUA1=0;MUTUA2=0;MUTUA3=0;MUTUA4=0;MUTUA5=0;MUTUA6=0;MUTUA7=0;MUTUA8=0;MUTUA9=0;
 MUTUA=SUBSTRING(MUTUA FROM 2 FOR (CHAR_LENGTH(MUTUA)-2));
 IF (MUTUA='0,') then begin 
   pMUTUAMAX=1000000000;pMUTUAMIN=0; 
 end else begin 
   pMUTUAMAX=0;pMUTUAMIN=0;
   MUTUA0=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   --pMUTUAMAX=MUTUA0;pMUTUAMIN=MUTUA0; 
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA1=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA2=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA3=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA5=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA5=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA6=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA7=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA8=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
   MUTUA=SUBSTRING(MUTUA FROM (POSITION(',' IN MUTUA)+1) FOR 100);
   IF (CHAR_LENGTH(MUTUA)>2) then begin 
     MUTUA9=CAST(SUBSTRING(MUTUA FROM 1 FOR (POSITION(',',MUTUA,1)-1)) AS INTEGER);
   end
 end

 /*IF (MUTUA>0) then begin 
   pMUTUAMAX=:MUTUA;pMUTUAMIN=:MUTUA;
 end else begin 
   pMUTUAMAX=1000000000;pMUTUAMIN=0;
 end */  

 IF (CENTRO>0) then begin 
   pCENTROMAX=:CENTRO;pCENTROMIN=:CENTRO;
 end else begin 
   pCENTROMAX=1000000000;pCENTROMIN=0;
 end
 
  IF (GPR>0) then begin 
   pGPRMAX=:GPR;pGPRMIN=:GPR;
 end else begin 
   pGPRMAX=3;pGPRMIN=0;
 end 
 
 IF (PAGADO IN ('T','F')) then begin 
   pPAGADO=:PAGADO;
 end else begin 
   pPagado='%';
 end
 
 IF (COLEGIADO>0) then begin
   pCOLEGIADOMIN=:COLEGIADO;pCOLEGIADOMAX=:COLEGIADO;
 end else begin
   pCOLEGIADOMIN=-1;pCOLEGIADOMAX=1000000000;
 end

 IF (MEDICO>0) then begin
   pMEDICOMIN=:MEDICO;pMEDICOMAX=:MEDICO;
 end else begin
   pMEDICOMIN=-1;pMEDICOMAX=1000000000;
 end
 
  IF (FACTURADO IN ('T','F')) then begin 
   pFACTURADO=:FACTURADO;
 end else begin 
   pFACTURADO='%';
 end
 
  IF (TIPOEXPLORACION>0) then begin
   pIOR_TIPOEXPLORACIONMIN=:TIPOEXPLORACION;pIOR_TIPOEXPLORACIONMAX=:TIPOEXPLORACION;
 end else begin
   pIOR_TIPOEXPLORACIONMIN=-1;pIOR_TIPOEXPLORACIONMAX=1000000000;
 end
 
  IF (TECNICO>0) then begin
   pIOR_TECNICOMIN=:TECNICO;pIOR_TECNICOMAX=:TECNICO;
 end else begin
   pIOR_TECNICOMIN=-1;pIOR_TECNICOMAX=1000000000;
 end
 
 DELETE FROM HFACTURACIONMENSUAL;
     
 for select 1 AS ORDEN,G.COD_GRUP,
        SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.cantidad, 0)) as sAnterior,
        SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.cantidad, 0)) as sActual,
        count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null)) as cAnterior,
        count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.oid, null)) as cActual ,
        (count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.oid, null)) - count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null))) as difCuenta,
        IIF(count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null))>0,
         (count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.oid, null)) - count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, 
         e.oid, null))) / count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null)) * 100,100) as difCuentaPorc
    from EXPLORACION e 
    JOIN GAPARATOS G ON G.OID = E.IOR_GRUPO
    where e.ESTADO = '3'  and (NOT e.IOR_GRUPO IN (10,18)) and (
    (E.FECHA between :FECHAINICIAL and  :FECHAFINAL) or(E.FECHA between :FINICIALPASSAT and  :FFINALPASSAT) )
    and (e.IOR_MEDICO BETWEEN :pMEDICOMIN and :pMEDICOMAX) and (E.IOR_COLEGIADO BETWEEN :pCOLEGIADOMIN and :pCOLEGIADOMAX) 
    and (e.OWNER BETWEEN :pCentroMin and :pCentroMax) 
    and ((e.IOR_ENTIDADPAGADORA BETWEEN :pMutuaMin and :pMutuaMax)
        OR e.IOR_ENTIDADPAGADORA IN (:MUTUA0,:MUTUA1,:MUTUA2,:MUTUA3,:MUTUA4,:MUTUA5,:MUTUA6,:MUTUA7,:MUTUA8,:MUTUA9))
    and (e.IOR_GPR BETWEEN :pGPRMin and :pGPRMax) and (e.PAGADO LIKE '%'||:pPagado) and (e.FACTURADA LIKE '%'||:pFACTURADO)
    and (e.IOR_TECNICO BETWEEN :pIOR_TECNICOMIN and :PIOR_TECNICOMAX)
    and (e.IOR_TIPOEXPLORACION BETWEEN :PIOR_TIPOEXPLORACIONMIN AND :PIOR_TIPOEXPLORACIONMAX)
    group by G.COD_GRUP
    --PLAN JOIN (SORT (JOIN ( E INDEX (IDXEXPLORACIONFECHA,IDXEXPLORACIONCOLEGIADO,IDXEXPLORACIONMEDICO, IDXEXPLORACIONMUTUAS), 
    --                    G INDEX (GAPARATOSPRIMARYKEY1))))
  
    UNION
    
    select 2 AS ORDEN,'TOTAL' AS COD_GRUP,
        SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.cantidad, 0)) as sAnterior,
        SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.cantidad, 0)) as sActual,
        count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null)) as cAnterior,
        count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.oid, null)) as cActual ,
        (count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.oid, null)) - count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null))) as difCuenta,
        IIF(count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null))>0,
         (count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, e.oid, null)) - count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, 
         e.oid, null))) / count(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, e.oid, null)) * 100,100) as difCuentaPorc
    from EXPLORACION e
    where e.ESTADO = '3'  and (NOT e.IOR_GRUPO IN (10,18)) and e.CANTIDAD > 0 and (
    (E.FECHA between :FECHAINICIAL and  :FECHAFINAL) or (E.FECHA between :FINICIALPASSAT and  :FFINALPASSAT) )
    and (e.OWNER BETWEEN :pCentroMin and :pCentroMax) 
    and ((e.IOR_ENTIDADPAGADORA BETWEEN :pMutuaMin and :pMutuaMax)
        OR e.IOR_ENTIDADPAGADORA IN (:MUTUA0,:MUTUA1,:MUTUA2,:MUTUA3,:MUTUA4,:MUTUA5,:MUTUA6,:MUTUA7,:MUTUA8,:MUTUA9))
    and (E.IOR_COLEGIADO BETWEEN :pCOLEGIADOMIN and :pCOLEGIADOMAX) and (e.IOR_MEDICO BETWEEN :pMEDICOMIN and :pMEDICOMAX)
    and (e.IOR_GPR BETWEEN :pGPRMin and :pGPRMax) and (e.PAGADO LIKE '%'||:pPagado) and (e.FACTURADA LIKE '%'||:pFACTURADO)
    and (e.IOR_TECNICO BETWEEN :pIOR_TECNICOMIN and :PIOR_TECNICOMAX)
    and (e.IOR_TIPOEXPLORACION BETWEEN :PIOR_TIPOEXPLORACIONMIN AND :PIOR_TIPOEXPLORACIONMAX)
    group by COD_GRUP 
    --PLAN JOIN (SORT (JOIN ( E INDEX (IDXEXPLORACIONFECHA,IDXEXPLORACIONCOLEGIADO,IDXEXPLORACIONMEDICO, IDXEXPLORACIONMUTUAS))))  
 

  INTO :ORDEN,:COD_GRUP,:SANTERIOR,:SACTUAL,:CANTERIOR,:CACTUAL,:DIFCUENTA,:DIFCUENTAPORC
 
  do begin
    INSERT INTO HFACTURACIONMENSUAL (ORDEN,COD_GRUP,SANTERIOR,SACTUAL,CANTERIOR,CACTUAL,DIFCUENTA,DIFCUENTAPORC)
       VALUES (:ORDEN,:COD_GRUP,:SANTERIOR,:SACTUAL,:CANTERIOR,:CACTUAL,:DIFCUENTA,:DIFCUENTAPORC);   
  end


  FOR SELECT G.COD_GRUP,
            SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, x.PRECIO, 0)) as sAnteriorConsum,
            SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, x.PRECIO, 0)) as sActualConsum  
    --from EXP_CONSUM X
    --JOIN EXPLORACION E ON E.OID=X.IOR_EXPLORACION
    from EXPLORACION E
    join EXP_CONSUM X ON X.IOR_EXPLORACION=E.OID 

    JOIN GAPARATOS G ON G.OID = E.IOR_GRUPO
    where e.ESTADO = '3'  and (NOT e.IOR_GRUPO IN (10,18)) and (
    (E.FECHA between :FECHAINICIAL and  :FECHAFINAL) or(E.FECHA between :FINICIALPASSAT and  :FFINALPASSAT) )
    and (e.OWNER BETWEEN :pCentroMin and :pCentroMax) 
    and ((e.IOR_ENTIDADPAGADORA BETWEEN :pMutuaMin and :pMutuaMax)
           OR e.IOR_ENTIDADPAGADORA IN (:MUTUA0,:MUTUA1,:MUTUA2,:MUTUA3,:MUTUA4,:MUTUA5,:MUTUA6,:MUTUA7,:MUTUA8,:MUTUA9))    
    and (e.IOR_GPR BETWEEN :pGPRMin and :pGPRMax) and (e.PAGADO LIKE '%'||:pPagado) and (e.FACTURADA LIKE '%'||:pFACTURADO)
    and (E.IOR_COLEGIADO BETWEEN :pCOLEGIADOMIN and :pCOLEGIADOMAX) and (e.IOR_MEDICO BETWEEN :pMEDICOMIN and :pMEDICOMAX)
    and (e.IOR_TECNICO BETWEEN :pIOR_TECNICOMIN and :PIOR_TECNICOMAX)
    and (e.IOR_TIPOEXPLORACION BETWEEN :PIOR_TIPOEXPLORACIONMIN AND :PIOR_TIPOEXPLORACIONMAX)
    group by G.COD_GRUP
    --PLAN SORT ((E INDEX (IDXEXPLORACIONFECHA,IDXEXPLORACIONMEDICO,IDXEXPLORACIONCOLEGIADO, IDXEXPLORACIONMUTUAS)),
    --    G INDEX (GAPARATOSPRIMARYKEY1), X INDEX (IDXEXP_CONSUMEXP))
    
    UNION
    
    SELECT 'TOTAL' AS COD_GRUP,
            SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYPASSAT, x.PRECIO, 0)) as sAnteriorConsum,
            SUM(IIF(EXTRACT(YEAR FROM E.FECHA) = :ANYACTUAL, x.PRECIO, 0)) as sActualConsum  
    --from EXP_CONSUM X
    --JOIN EXPLORACION E ON E.OID=X.IOR_EXPLORACION
    from EXPLORACION E
    join EXP_CONSUM X ON X.IOR_EXPLORACION=E.OID 

    where e.ESTADO = '3'  and (NOT e.IOR_GRUPO IN (10,18)) and (
    (E.FECHA between :FECHAINICIAL and  :FECHAFINAL) or(E.FECHA between :FINICIALPASSAT and  :FFINALPASSAT) )
    and (e.OWNER BETWEEN :pCentroMin and :pCentroMax) 
    and ((e.IOR_ENTIDADPAGADORA BETWEEN :pMutuaMin and :pMutuaMax)
           OR e.IOR_ENTIDADPAGADORA IN (:MUTUA0,:MUTUA1,:MUTUA2,:MUTUA3,:MUTUA4,:MUTUA5,:MUTUA6,:MUTUA7,:MUTUA8,:MUTUA9))    
    and (e.IOR_GPR BETWEEN :pGPRMin and :pGPRMax) and (e.PAGADO LIKE '%'||:pPagado) and (e.FACTURADA LIKE '%'||:pFACTURADO)
    and (E.IOR_COLEGIADO BETWEEN :pCOLEGIADOMIN and :pCOLEGIADOMAX) and (e.IOR_MEDICO BETWEEN :pMEDICOMIN and :pMEDICOMAX)
    and (e.IOR_TECNICO BETWEEN :pIOR_TECNICOMIN and :PIOR_TECNICOMAX)
    and (e.IOR_TIPOEXPLORACION BETWEEN :PIOR_TIPOEXPLORACIONMIN AND :PIOR_TIPOEXPLORACIONMAX)    
    group by COD_GRUP
    --PLAN SORT ((E INDEX (IDXEXPLORACIONFECHA,IDXEXPLORACIONMEDICO,IDXEXPLORACIONCOLEGIADO, IDXEXPLORACIONMUTUAS)),
    --    X INDEX (IDXEXP_CONSUMEXP))

  INTO :COD_GRUP,:SANTERIORCONSUM,SACTUALCONSUM
  do begin 
    UPDATE HFACTURACIONMENSUAL SET SANTERIORCONSUM=:SANTERIORCONSUM, SACTUALCONSUM=:SACTUALCONSUM
        WHERE COD_GRUP=:COD_GRUP;
  end

  FOR SELECT ORDEN,COD_GRUP,SANTERIOR,SANTERIORCONSUM,
           (SANTERIOR+COALESCE(SANTERIORCONSUM,0)) AS SANTERIORTOTAL,
           SACTUAL,SACTUALCONSUM,(SACTUAL+COALESCE(SACTUALCONSUM,0)) AS SACTUALTOTAL,
           ((SACTUAL+COALESCE(SACTUALCONSUM,0))-(SANTERIOR+COALESCE(SANTERIORCONSUM,0))) AS DIFCANTIDAD,
           IIF((SANTERIOR+COALESCE(SANTERIORCONSUM,0))>0,(((SACTUAL+COALESCE(SACTUALCONSUM,0))-
           (SANTERIOR+COALESCE(SANTERIORCONSUM,0)))/(SANTERIOR+COALESCE(SANTERIORCONSUM,0))*100),100) AS DIFCANTIDADPORC,
           CANTERIOR,CACTUAL,DIFCUENTA,DIFCUENTAPORC
    FROM HFACTURACIONMENSUAL
    ORDER BY 3,1,2
    INTO :ORDEN,:COD_GRUP,:SANTERIOR,:SANTERIORCONSUM,:SANTERIORTOTAL,:SACTUAL,:SACTUALCONSUM,:SACTUALTOTAL,:DIFCANTIDAD,:DIFCANTIDADPORC,:CANTERIOR,:CACTUAL,:DIFCUENTA,:DIFCUENTAPORC



DO BEGIN
  ORDEN=:ORDEN;
  COD_GRUP=:COD_GRUP;
  SANTERIOR=:SANTERIOR;
  SANTERIORCONSUM=:SANTERIORCONSUM;
  SANTERIORTOTAL=:SANTERIORTOTAL;
  SACTUAL=:SACTUAL;
  SACTUALCONSUM=:SACTUALCONSUM;
  SACTUALTOTAL=:SACTUALTOTAL;
  DIFCANTIDAD=:DIFCANTIDAD;
  DIFCANTIDADPORC=:DIFCANTIDADPORC;
  CANTERIOR=:CANTERIOR;
  CACTUAL=:CACTUAL;
  DIFCUENTA=:DIFCUENTA;
  DIFCUENTAPORC=:DIFCUENTAPORC;
  SUSPEND;
END
END
^
SET TERM ; ^


GRANT EXECUTE
 ON PROCEDURE FACTURACION TO  AFFIDEA;

GRANT EXECUTE
 ON PROCEDURE FACTURACION TO  SYSDBA;

