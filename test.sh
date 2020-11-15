#!/bin/bash

today=$(date +"%Y%m%d")
date2=$(date --date='1 days ago' +%Y%m%d)  
fileDate=$(date +"%Y%m%d" -r ssl_apache_20201115_logs)

echo $today
echo $fileDate

logPath='D:\Space\PersonData\Desktop\WorkSpace\ShellTest\'
tempBasePath='D:\Space\PersonData\Desktop\WorkSpace\ShellTest\'

filelist=("ssl_apache_"$today"_logs" "7280_apache_"$today"_logs" )
port=( '443' '7280' )

for (( i = 0 ; i < ${#filelist[@]} ; i++ )) 
do
   file=`ls -ltr | awk '{print $9}' | grep "^"${filelist[$i]}"$"`
   if [ $file ]
   then            
      splitPath=$tempBasePath'tmp_'${port[$i]}'\'
      mkdir -p $splitPath

      oldFile=`"C:\Program Files\Git\usr\bin\find.exe" $splitPath -name "*.part*" -type f -cmin 30 | head -1`
      if [ $oldFile ]
      then
         "C:\Program Files\Git\usr\bin\find.exe" $splitPath -name "*.part*" -type f -cmin 30 -exec rm -f {} \; 
         echo 'delete old file'
      fi

      split -d -l 5000 $logPath${filelist[$i]} $splitPath${filelist[$i]}".part"

      index=0
      postFile=`ls -r $splitPath | head -2`
      for f in $postFile
      do       
         if [ $index != 0 ] 
         then
            sleep 3
            echo 'sleep end'
         fi 
         echo $splitPath$f

         index=index+1
      done

   fi  
done

# "C:\Program Files\Git\usr\bin\find.exe" "D:\Space\PersonData\Desktop\WorkSpace\ShellTest\tmp_7280" -type f -printf '%T+ %p\n'
