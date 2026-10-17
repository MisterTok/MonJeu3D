#!/bin/bash
# Recompile le projet dans l'éditeur ouvert, lance Play puis exécute des commandes de test / captures (via ../_dl).
# Usage : Tools/remote_shots.sh [--norebuild] cmd1 cmd2 ...   (cmd = commande de test « open:shop », « item6 »… ou « shot:nom » pour une capture)
DL=/c/jeu/_dl
DLL=/c/jeu/MonJeu3D/Library/ScriptAssemblies/Assembly-CSharp.dll
LOG=/c/Users/pierr/AppData/Local/Unity/Editor/Editor.log
if [ "$1" != "--norebuild" ]; then
  touch $DL/stop_request; sleep 4
  before=$(stat -c %Y $DLL)
  touch $DL/refresh_request
  for i in $(seq 1 60); do sleep 3; [ $(stat -c %Y $DLL) -gt $before ] && break; grep -aq "error CS" <(tail -c 200000 $LOG) && break; done
  errs=$(tail -c 400000 $LOG | tr -cd '\11\12\15\40-\176' | grep "error CS" | sort -u)
  if [ -n "$errs" ]; then echo "$errs"; exit 1; fi
  sleep 6; touch $DL/play_request; sleep 25
else shift 0; fi
[ "$1" == "--norebuild" ] && shift
for c in "$@"; do
  if [[ $c == shot:* ]]; then
    n=${c#shot:}; rm -f $DL/$n.png; echo $n > $DL/shot_request
    for i in $(seq 1 15); do [ -f $DL/$n.png ] && break; sleep 1; done; echo "capture $n"
  else
    echo "$c" > $DL/shot_request; sleep 2.5
  fi
done
tail -c 100000 $LOG | tr -cd '\11\12\15\40-\176' | grep -A2 "Exception" | sort -u | head -8
