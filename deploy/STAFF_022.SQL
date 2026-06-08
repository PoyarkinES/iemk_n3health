//14.02.2022	Vladimir Soloviev		SIR# DT00029859  File:PSTAFF_022.SQL

Alter table dba.staff
   add snils varchar(20)
       null;

comment on column dba.staff.snils is 
'N3.Health procedure code';

INSERT INTO dba.pbcatcol VALUES ( 'staff', NULL,	'dba', 
'snilsode',	NULL,	'snils',	23,	'snils', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'SNILS', NULL,	NULL); 

comment on column dba.staff.snils is 
'SNILS';

