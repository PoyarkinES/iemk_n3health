//10.02.2022	Vladimir Soloviev		SIR# DT00029859  File:PROC_033.SQL

create table dba.n3h_dict 
(
   id                   integer                        not null default autoincrement,
   dict_id              integer                        not null,
   code                 varchar(255)                   not null,
   name                 varchar(1024)                  not null,
   sort_order           integer                        not null default 0,
   status               smallint                       not null default 1,
   constraint PK_n3h_dict primary key (id)
);

comment on table dba.n3h_dict is 
'N3.Health dictionaries';

comment on column dba.n3h_dict.id is 
'Therm ID';

comment on column dba.n3h_dict.dict_id is 
'Dictionary ID';

comment on column dba.n3h_dict.code is 
'Therm code';

comment on column dba.n3h_dict.name is 
'Therm name';

comment on column dba.n3h_dict.sort_order is 
'Therm sorting order';

comment on column dba.n3h_dict.status is 
'Therm status (0=inactive)';

INSERT INTO dba.pbcattbl VALUES ( 
'n3h_dict', 
NULL,	'dba',	-9,	400,	'N',	'N',	0,	34,	'Arial',	-9,	400,	'N',	'N',	0,	34,	'Arial',	-9,	400,	'N',	'N',	0,	34,	'Arial', 
'N3.Health dictionaries'); 

INSERT INTO dba.pbcatcol VALUES ( 'n3h_dict', NULL,	'dba', 
'id',	NULL,	'id',	23,	'id', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'Therm ID', NULL,	NULL); 

INSERT INTO dba.pbcatcol VALUES ( 'n3h_dict', NULL,	'dba', 
'dict_id',	NULL,	'dict_id',	23,	'dict_id', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'Dictionary ID', NULL,	NULL); 

INSERT INTO dba.pbcatcol VALUES ( 'n3h_dict', NULL,	'dba', 
'code',	NULL,	'code',	23,	'code', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'Therm code', NULL,	NULL); 

INSERT INTO dba.pbcatcol VALUES ( 'n3h_dict', NULL,	'dba', 
'name',	NULL,	'name',	23,	'name', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'Therm name', NULL,	NULL); 

INSERT INTO dba.pbcatcol VALUES ( 'n3h_dict', NULL,	'dba', 
'sort_order',	NULL,	'sort_order',	23,	'sort_order', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'Therm sort order', NULL,	NULL); 

INSERT INTO dba.pbcatcol VALUES ( 'n3h_dict', NULL,	'dba', 
'status',	NULL,	'status',	23,	'status', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'Therm status (0=inactive)', NULL,	NULL); 

Alter table dba.practice_services
   add n3h_dict_id integer
       null;

comment on column dba.practice_services.n3h_dict_id is 
'N3.Health dictionary reference';

INSERT INTO dba.pbcatcol VALUES ( 'practice_services', NULL,	'dba', 
'n3h_dict_id',	NULL,	'n3h_dict_id',	23,	'n3h_dict_id', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'N3.Health dictionary reference', NULL,	NULL); 

Alter table dba.procedures
   add n3h_code varchar(20)
       null;

comment on column dba.procedures.n3h_code is 
'N3.Health procedure code';

INSERT INTO dba.pbcatcol VALUES ( 'procedures', NULL,	'dba', 
'n3h_code',	NULL,	'n3h_code',	23,	'n3h_code', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'N3.Health procedure code', NULL,	NULL); 

Alter table dba.staff_positions
   add n3h_dict_id integer
       null;

comment on column dba.staff_positions.n3h_dict_id is 
'N3.Health dictionary reference';

INSERT INTO dba.pbcatcol VALUES ( 'staff_positions', NULL,	'dba', 
'n3h_dict_id',	NULL,	'n3h_dict_id',	23,	'n3h_dict_id', 25,	25,	'[General]',	0,	0,	0,	NULL,	'N',	NULL, 
'N3.Health dictionary reference', NULL,	NULL); 

alter table dba.practice_services
   add constraint FK_PRACT_SERV_REF_N3H_DICT foreign key (n3h_dict_id)
      references n3h_dict (id)
      on update cascade
      on delete set null;

alter table dba.staff_positions
   add constraint FK_STAFF_POS_REF_N3H_DICT foreign key (n3h_dict_id)
      references n3h_dict (id)
      on update cascade
      on delete set null;


Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'10234', 
'заведующий (начальник) структурного подразделения (отдела, отделения, лаборатории, кабинета, отряда и другое) медицинской организации - врач-специалист' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'10235', 
'заведующий (главный врач, начальник) структурного подразделения, осуществляющего медицинскую деятельность, иной организации'  );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'191', 
'врач-стоматолог' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'142', 
'гигиенист стоматологический'  );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'192', 
'врач-стоматолог детский'  );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'195', 
'врач-стоматолог-хирург'  );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'194', 
'врач-стоматолог-терапевт' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'193', 
'врач-стоматолог-ортопед' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'196', 
'врач-ортодонт'  );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'201', 
'врач-челюстно-лицевой хирург' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'62', 
'врач-рентгенолог' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (1,
'87', 
'врач-физиотерапевт' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.059', 
'Гемисекция зуба' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.054', 
'Внутрикостная дентальная имплантация' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.056', 
'Восстановление целостности зубного ряда несъемным консольным протезом' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.058', 
'Лечение перикоронита (промывание, рассечение и/или иссечение капюшона)' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.050', 
'Профессиональное отбеливание зубов' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.052', 
'Восстановление зуба штифтовым зубом' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.053', 
'Снятие несъемной ортопедической конструкции' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.051', 
'Профессиональная гигиена полости рта и зубов' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.055', 
'Синус-лифтинг (костная пластика, остеопластика)' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.057', 
'Запечатывание фиссуры зуба герметиком' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.053.002', 
'Распил ортодонтического аппарата через винт' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (2,
'A16.07.053.001', 
'Снятие, постановка коронки, кольца ортодонтических' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'3', 
'Стоматология' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'173', 
'Ортодонтия' );

select * from dba.n3h_dict

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'271', 
'Рентгенология' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'209', 
'Стоматология ортопедическая' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'174', 
'Стоматология детская' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'233', 
'Стоматология хирургическая' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'171', 
'Стоматология общей практики' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'208', 
'Стоматология' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'175', 
'Стоматология ортопедическая' );

Insert Into dba.n3h_dict ( dict_id, code, name ) values (3,
'176', 
'Стоматология терапевтическая' );

