# TodaysAbsences

This repo is responsible for sending the Daily Absences messages to the #general slack channel.

## Bob

Because the /timeoff endpoint on the Bob api doesn't provide an employee's department, an additional call is made for each employee to the /people endpoint - this also allows us to fetch an employee's squad (if they are assigned one) from the `work.custom.Squad_5Gqot` field.