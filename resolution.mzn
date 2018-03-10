include "globals.mzn";

%data
int: nbTrajet;
int: nbStep;
int: bonus;
int: nbVehicule;
array [1..nbTrajet] of 1..nbStep: startStep;
array [1..nbTrajet] of 1..nbStep: endStep;
array [1..nbTrajet, 1..nbTrajet] of int: distanceToNextRide;
array [1..nbTrajet] of int : distanceFrom0;
array [1..nbTrajet] of int : distance;

%variable
array [1..nbVehicule, 1..nbTrajet] of var bool : trajetTaken;
array [1..nbTrajet] of var 0..nbVehicule : corresTrajetVehicule;
array [1..nbVehicule, 1..nbTrajet] of var 0..nbStep : startTime;
array [1..nbVehicule, 1..nbTrajet] of var 0..nbStep : endTime;
array [1..nbVehicule, 1..nbTrajet] of var 0..nbTrajet : precTrajet;
array [1..nbVehicule] of var 1..nbTrajet: firstTrip;
var int: score;

%constraint

%Le premier trajet doit être pris est affecté au bon véhicule
constraint forall(v in 1..nbVehicule)(trajetTaken[v,firstTrip[v]] /\ corresTrajetVehicule[firstTrip[v]]==v);

%Le premier trajet n'a pas de de précédent
constraint forall(v in 1..nbVehicule)(precTrajet[v,firstTrip[v]]=0);

%Le début du premier trajet est la distance depuis 0,0
constraint forall(v in 1..nbVehicule)(startTime[v,firstTrip[v]]>=distanceFrom0[firstTrip[v]]);

%Tous les premiers trajets doivent être différent
constraint alldifferent_except_0(firstTrip);

%Tous les précédents doivent être différent
constraint alldifferent_except_0(array1d(precTrajet));

%Pour chaque trajet pris, la fin ne peut pas être après la fin du jeu
constraint forall(v in 1..nbVehicule, t in 1..nbTrajet)(endTime[v,t]<endStep[t]);

%Pour chaque trajet pris, le début ne peut pas commencer avant le vrai début
constraint forall(v in 1..nbVehicule, t in 1..nbTrajet)(trajetTaken[v,t]->startTime[v,t]>=startStep[t]);

%Pour chaque trajet, s'il est pris, la fin doit correspondre au début plus la distance
constraint forall(v in 1..nbVehicule, t in 1..nbTrajet)(trajetTaken[v,t]-> endTime[v,t]=startTime[v,t]+distance[t]);

%Pour chaque trajet, s'il est pris par un véhicule alors il est assigné
constraint forall(v in 1..nbVehicule, t in 1..nbTrajet)(trajetTaken[v,t]->corresTrajetVehicule[t]=v);

%Pour chaque trajet pris, la date de début doit être après la date de fin du précédent + le temps d'aller au trajet
constraint forall(v in 1..nbVehicule, t in 1..nbTrajet)((trajetTaken[v,t] /\ firstTrip[v]!=t)->startTime[v,t]>=endTime[v,precTrajet[v,t]]+distanceToNextRide[t,precTrajet[v,t]]);

%Le précédent d'un trajet pris doit être pris
constraint forall(v in 1..nbVehicule, t in 1..nbTrajet)(trajetTaken[v,t]-> trajetTaken[v,precTrajet[v,t]] \/ firstTrip[v]==t);

%maximiser somme pour chaque vehicule pour chaque trajet si trajetTaken[vehicule, trajet] alors distance[trajet]+startTime[vehicule,trajet]==startStep[trajet]?bonus:0 sinon 0
constraint score=sum(v in 1..nbVehicule, t in 1..nbTrajet)(if(trajetTaken[v,t]) then distance[t]+if(startTime[v,t]==startStep[t])then bonus else 0 endif else 0 endif);

solve ::int_search(corresTrajetVehicule, first_fail, indomain, complete) maximize(score);

%print cette somme
output["Score :", show(score),"\n Corres",show(corresTrajetVehicule),"\n TrajetTaken",show(trajetTaken), "\n StartTime",show(startTime),"\n EndTime",show(endTime),"\n PrecTrajet",show(precTrajet),"\n FirstTrip",show(firstTrip)];