SET TERM ^ ;
ALTER PROCEDURE CITAS_TUOTEMPO (
    OID_PACIENTE integer,
    OID_GAPARATO integer,
    OID_EXPLO integer,
    FECHA_INI timestamp,
    FECHA_FIN timestamp )
RETURNS (
    APP_LID integer,
    CREATED varchar(25),
    CANCELLED varchar(1),
    MODIFIED varchar(20),
    STATUS varchar(15),
    CHECKEDIN varchar(25),
    START_VISIT varchar(25),
    END_VISIT varchar(25),
    PREPARATION varchar(1000),
    REMINDER varchar(1),
    DISABLE_RESCHEDULE varchar(1),
    DISABLE_CANCEL varchar(1),
    AVAILABILITY_LID varchar(50),
    ADATE varchar(10),
    START_TIME varchar(5),
    END_TIME varchar(5),
    LOCATION_LID integer,
    RESOURCE_LID integer,
    ACTIVITY_LID integer,
    INSURANCE_LID integer,
    PRICE numeric(8,2),
    USER_LID integer,
    ID_NUMBER varchar(25),
    ID_TYPE smallint,
    FIRST_NAME varchar(30),
    SECOND_NAME varchar(60),
    THIRD_NAME varchar(60),
    BIRTHDATE varchar(10),
    PLACE_OF_BIRTH varchar(10),
    GENDER varchar(1),
    CONTACT_EMAIL varchar(50),
    CONTACT_MOBILE varchar(20),
    CONTACT_WORK varchar(10),
    PSMS varchar(1),
    PEMAIL varchar(1),
    PPHONE varchar(1),
    PPRIMARY varchar(1),
    PPROMOTIONS varchar(1),
    PREVIEW varchar(1),
    PDOSSIER varchar(1),
    STREET_NUMBER varchar(10),
    STREET varchar(200),
    ZIPCODE varchar(10),
    CITY varchar(50),
    PROVINCE varchar(50),
    REGION varchar(50),
    COUNTRY varchar(50) )
AS
DECLARE VARIABLE TIPO SMALLINT;
DECLARE VARIABLE SQL1 VARCHAR(5000);
DECLARE VARIABLE SQL2 VARCHAR(1000);
BEGIN
/* Primer paso averiguar cual de las cuatro funciones se trata
1-ENDPOINT 1 PASAN PACIENTE Y DOS FECHAS: CITAS DE UN PAC ENTRE FEC6HAS
2-ENDOPOINT 2 PASAN SOLO UNA FECHA: TODAS LAS CITAS C9ON CREACION >FECHA
3-ENDPOINT 3 PASAN APARATO Y DOS FECHAS: CITAS DE UN APARATO ENTRE FECHAS/
4-ENDPOINT 4 PASAN SOLO UNA EXPLO : DATOS DE LA EXPLO
5-ENDPOINT 5 PASAN DOS FECHAS Y QUIEREN LAS CITAS NOT ATTENDED
*/
IF (OID_PACIENTE>0) THEN TIPO=1;
ELSE IF (OID_GAPARATO>0) THEN TIPO=3;
ELSE IF (OID_EXPLO>0) THEN TIPO=4;
ELSE IF (FECHA_FIN=NULL) THEN TIPO=2;
ELSE TIPO=5;



SQL1=
    'SELECT E.OID AS APP_LID,
    LPAD(EXTRACT(DAY FROM E.FECHA_IDEN),2,0)||''/''||
    LPAD(EXTRACT(MONTH FROM E.FECHA_IDEN),2,0)||''/''||
    EXTRACT(YEAR FROM E.FECHA_IDEN)||'' ''||
    LPAD(EXTRACT(HOUR FROM CAST(''01-01-1000 ''||E.HORA_IDEN AS TIMESTAMP)),2,0)||'':''||
    LPAD(EXTRACT(MINUTE FROM CAST(''01-01-1000 ''||E.HORA_IDEN AS TIMESTAMP)),2,0)||'':00''
    AS CREATED,
    IIF(E.ESTADO IN (''1'',''4'',''5''),''T'',''F'') as CANCELLED,
    E.HORAMOD AS MODIFIED,
    CASE  
    WHEN E.ESTADO IN (''1'',''4'',''5'')  THEN ''Cancelled''
    WHEN (E.ESTADO=''3'') THEN ''Completed''
    WHEN (E.ESTADO=''0'' AND E.FECHA<CURRENT_TIMESTAMP) THEN ''Confirmed''
    WHEN (E.ESTADO=''0'' AND E.FECHA>CURRENT_TIMESTAMP) THEN ''NotAttended''
    ELSE '' ''
    END AS STATUS,
    IIF(((E.HORA_LL  IS NULL) OR (E.ESTADO IN (''1'',''4'',''5''))),NULL,
    LPAD(EXTRACT(DAY FROM E.FECHA),2,0)||''/''||
    LPAD(EXTRACT(MONTH FROM E.FECHA),2,0)||''/''||
    EXTRACT(YEAR FROM E.FECHA)||'' ''||
    LPAD(EXTRACT(HOUR FROM CAST(''01-01-1000 ''||E.HORA_LL AS TIMESTAMP)),2,0)||'':''||
    LPAD(EXTRACT(MINUTE FROM CAST(''01-01-1000 ''||E.HORA_LL AS TIMESTAMP)),2,0)||'':00'')
    AS CHECKEDIN,
    IIF((E.ESTADO<>''3''),NULL,
    LPAD(EXTRACT(DAY FROM E.FECHA),2,0)||''/''||
    LPAD(EXTRACT(MONTH FROM E.FECHA),2,0)||''/''||
    EXTRACT(YEAR FROM E.FECHA)||'' ''||
    LPAD(EXTRACT(HOUR FROM CAST(''01-01-1000 ''||E.HORA_EX AS TIMESTAMP)),2,0)||'':''||
    LPAD(EXTRACT(MINUTE FROM CAST(''01-01-1000 ''||E.HORA_EX AS TIMESTAMP)),2,0)||'':00'')
    AS STAR_VISIT,
    IIF((E.ESTADO<>''3''),NULL,
    LPAD(EXTRACT(DAY FROM E.FECHA),2,0)||''/''||
    LPAD(EXTRACT(MONTH FROM E.FECHA),2,0)||''/''||
    EXTRACT(YEAR FROM E.FECHA)||'' ''||
    LPAD(EXTRACT(HOUR FROM CAST(''01-01-1000 ''||E.HORA_EX AS TIMESTAMP)),2,0)||'':''||
    LPAD(EXTRACT(MINUTE FROM CAST(''01-01-1000 ''||E.HORA_EX AS TIMESTAMP)),2,0)||'':00'')
    AS END_VISIT, 
    TRIM(A.TEXTO) AS PREPARATION,
    ''F'' AS REMINDER,
    ''F'' AS DISABLE_RESCHEDULE,
    ''F'' AS DISABLE_CANCEL,
    EXTRACT(YEAR FROM E.FECHA)||LPAD(EXTRACT(MONTH FROM E.FECHA),2,0)||
    LPAD(EXTRACT(DAY FROM E.FECHA),2,0)||SUBSTRING(E.HORA FROM 1 FOR 2)|| 
    SUBSTRING(E.HORA FROM 4 FOR 2)||''_''||E.IOR_APARATO||''_''||E.IOR_APARATO AS AVAILABILITY_LID,

    LPAD(EXTRACT(DAY FROM E.FECHA),2,0)||''/''||
    LPAD(EXTRACT(MONTH FROM E.FECHA),2,0)||''/''||
    EXTRACT(YEAR FROM E.FECHA) AS ADATE,
    
    E.HORA AS START_TIME,
    E.HORA AS END_TIME,
    E.OWNER AS LOCATION_LID,
    E.IOR_APARATO AS RESOURCE_LID,
    E.IOR_TIPOEXPLORACION AS ACTIVITY_LID,
    E.IOR_ENTIDADPAGADORA AS INSURANCE_LID,
    E.CANTIDAD AS PRICE, 
    
    P.OID AS  USER_LID,
    P.DNI AS ID_NUMBER,
    ''1'' AS ID_TYPE,
    IIF(POSITION('','',P.PACIENTE)>0,
    SUBSTRING(P.PACIENTE FROM (POSITION('','',P.PACIENTE)+1) FOR 20),'''') AS   FIRST_NAME,
    IIF(POSITION('','',P.PACIENTE)>0,
    SUBSTRING(P.PACIENTE FROM 1 FOR (POSITION('','',P.PACIENTE)-1)),
    P.PACIENTE)  AS SECOND_NAME,
    '' '' AS THIRD_NAME,
    LPAD(EXTRACT(DAY FROM P.FECHAN),2,0)||''/''||
    LPAD(EXTRACT(MONTH FROM P.FECHAN),2,0)||''/''||
    EXTRACT(YEAR FROM P.FECHAN) AS   BIRTHDATE,
    '' '' AS PLACE_OF_BIRTH,
    P.SEXO AS GENDER,
    P.EMAIL AS CONTACT_EMAIL,
    T.NUMERO AS CONTACT_MOBILE,
    '' '' AS CONTACT_WORK,
    
    ''T'' AS PSMS,
    ''T'' AS PEMAIL,
    ''F'' AS PPHONE,
    ''T'' AS PPRIMARY,
    ''T'' AS PPROMOTIONS,
    ''T'' AS PREVIEW,
    ''T'' AS PDOSSIER,    
      '''' AS STREET_NUMBER,
      D.DIRECCION AS STREET,
      D.CP AS ZIPCODE,
      D.POBLACION AS CITY,
      D.PROVINCIA AS PROVINCE,
      '''' AS REGION,
      D.PAIS AS COUNTRY

    
    FROM EXPLORACION E 
    JOIN APARATOS A ON A.OID=E.IOR_TIPOEXPLORACION
    JOIN PACIENTE P ON P.OID=E.IOR_PACIENTE
    JOIN MUTUAS M ON M.OID=E.IOR_ENTIDADPAGADORA
    LEFT JOIN DIRECCION D ON D.OID=(SELECT FIRST 1 DD.OID FROM DIRECCION DD WHERE DD.OWNER=P.OID
                                                                    ORDER BY DD.OID DESC)
    LEFT JOIN TELEFONO T ON T.OID=(SELECT FIRST 1 TT.OID FROM TELEFONO TT 
              WHERE TT.OWNER=P.OID 
              AND (SUBSTRING(TRIM(TT.NUMERO) FROM 1 FOR 1)) IN (''6'',''7''))  ';

   
    IF (TIPO=1) THEN SQL2='WHERE E.IOR_PACIENTE='||:OID_PACIENTE||' AND M.TUOTEMPO=''T'' AND E.FECHA BETWEEN '''||:FECHA_INI||''' AND '''||:FECHA_FIN||'''';
    ELSE IF (TIPO=2) THEN SQL2='WHERE  M.TUOTEMPO=''T'' AND E.FECHA>'''||:FECHA_INI||'''';
    ELSE IF (TIPO=3) THEN SQL2='WHERE  M.TUOTEMPO=''T'' AND E.IOR_GRUPO='||:OID_GAPARATO||
                 ' AND E.FECHA BETWEEN '''||:FECHA_INI||''' AND '''||:FECHA_FIN||'''';    
    --ELSE IF (TIPO=4) THEN SQL2='WHERE  M.TUOTEMPO=''T'' AND E.OID='||:OID_EXPLO;
    ELSE IF (TIPO=4) THEN SQL2='WHERE E.OID='||:OID_EXPLO;
    ELSE IF (TIPO=5) THEN SQL2='WHERE M.TUOTEMPO=''T'' and E.FECHA BETWEEN '''||:FECHA_INI||''' AND '''||CURRENT_TIMESTAMP||'''  AND E.ESTADO=''0''';    

      
    FOR EXECUTE STATEMENT SQL1||SQL2
          -- Insertar el valor de SQL1 || SQL2 en la tabla de log
             
    INTO :APP_LID,:CREATED,:CANCELLED,:MODIFIED,:STATUS,:CHECKEDIN,
              :START_VISIT,:END_VISIT,:PREPARATION,:REMINDER,
              :DISABLE_RESCHEDULE,:DISABLE_CANCEL,:AVAILABILITY_LID,
              :ADATE,:START_TIME,:END_TIME,:LOCATION_LID,:RESOURCE_LID,
              :ACTIVITY_LID,:INSURANCE_LID,:PRICE,:USER_LID,:ID_NUMBER,
             :ID_TYPE,:FIRST_NAME,:SECOND_NAME,:THIRD_NAME,:BIRTHDATE,
             :PLACE_OF_BIRTH,:GENDER,:CONTACT_EMAIL,:CONTACT_MOBILE,
             :CONTACT_WORK,:PSMS,:PEMAIL,:PPHONE,:PPRIMARY,
             :PPROMOTIONS,:PREVIEW,:PDOSSIER, 
             :STREET_NUMBER, :STREET, :ZIPCODE,:CITY,:PROVINCE,:REGION,:COUNTRY
    
    DO BEGIN
         
   
        SUSPEND;
    END
END^
SET TERM ; ^


GRANT EXECUTE
 ON PROCEDURE CITAS_TUOTEMPO TO  SYSDBA;
