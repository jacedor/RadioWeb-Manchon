﻿INSERT INTO UCCADMENU
(ID, IDUSER, DESCRIPCION, ICONO, IDPARENT, ORDEN, URL)
VALUES(gen_id(GENUID,1), -2, 'TVs', 'viewTVs', 7, 4, '/kiosko/tv');


INSERT INTO UCCADMENU
(ID, IDUSER, DESCRIPCION, ICONO, IDPARENT, ORDEN, URL)
VALUES(gen_id(GENUID,1), -2, 'TV - Agenda', 'viewTVAgendas', 7, 5, '/kiosko/agendas');



CREATE TABLE KIOSKO_TV (
	OID	INTEGER NOT NULL,
	ID VARCHAR(100) NOT NULL,
	NOMBRE VARCHAR(255),
	DESCRIPCION VARCHAR(2000),
	DESHABILITADA INTEGER DEFAULT 0 NOT NULL
);

CREATE UNIQUE INDEX KIOSKO_TV_PK ON KIOSKO_TV (OID);


SET TERM ^ ;
CREATE TRIGGER TRIG_KIOSKO_TV_OID FOR KIOSKO_TV
ACTIVE BEFORE INSERT POSITION 0 
AS 
BEGIN 
   if (new.OID is null) then new.OID = gen_id(GENUID,1);
END^
SET TERM ; ^


CREATE TABLE KIOSKO_DAPARATO_TV (
	OID	INTEGER NOT NULL,
	DAPARATO_OID INTEGER,
	TV_OID INTEGER
);
CREATE UNIQUE INDEX KIOSKO_DAPARATO_TV_PK ON KIOSKO_DAPARATO_TV (OID);

SET TERM ^ ;
CREATE TRIGGER TRIG_KIOSKO_DAPARATO_TV_OID FOR KIOSKO_DAPARATO_TV
ACTIVE BEFORE INSERT POSITION 0 
AS 
BEGIN 
   if (new.OID is null) then new.OID = gen_id(GENUID,1);
END^
SET TERM ; ^


--Exploracion
ALTER TABLE EXPLORACION ADD TICKET_KIOSKO VARCHAR(100);


INSERT INTO WEBCONFIG (OID, VERS, OWNER, CLAVE, VALOR, SECCION, DESCRIPCION) 
VALUES(gen_id(GENUID,1), 0, NULL, 'RutaKiosko', 'http://localhost/', 'RUTAS', NULL);
INSERT INTO WEBCONFIG (OID, VERS, OWNER, CLAVE, VALOR, SECCION, DESCRIPCION) 
VALUES(gen_id(GENUID,1), 1, NULL, 'RutaKiosko', 'http://localhost/', 'RUTAS', NULL);