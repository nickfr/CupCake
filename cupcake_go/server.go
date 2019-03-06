package main

import (
	"database/sql"
	"fmt"
	"log"
	"net/http"
	"os"
	"strconv"
	"sync"

	_ "github.com/lib/pq"
)

var mu sync.Mutex
var count int

func main() {

	http.HandleFunc("/get", get)
	log.Fatal(http.ListenAndServe("localhost:8000", nil))
}

func get(w http.ResponseWriter, r *http.Request) {
	pid, num := os.Getpid(), getID()

	db_server := getEnv("CUPCAKE_DBSERVER", "localhost")
	db_port, err := strconv.ParseInt(getEnv("CUPCAKE_DBPORT", "5432"), 10, 32)
	if err != nil {
		panic(err)
	}
	db_name := getEnv("CUPCAKE_DBNAME", "postgres")
	db_user := getEnv("CUPCAKE_DBUSER", "postgres")
	db_pwd := getEnv("CUPCAKE_DBPASSWORD", "password1")
	fmt.Printf("Database on %s\n", db_server)
	fmt.Printf("%d calling %d\n", pid, num)

	conn := getConnectionString(db_server, int(db_port), db_user, db_pwd, db_name)
	db, err := sql.Open("postgres", conn)
	if err != nil {
		panic(err)
	}
	defer db.Close()

	rows, err := db.Query("select * from get_data();")
	if err != nil {
		// handle this error better than this
		panic(err)
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	fmt.Fprintf(w, "[")

	defer rows.Close()
	for rows.Next() {
		var id int
		var data string
		err = rows.Scan(&id, &data)
		if err != nil {
			// handle this error
			panic(err)
		}
		fmt.Fprintf(w, "\"%s\",", data)
	}
	fmt.Fprintf(w, "]")
	fmt.Printf("%d done %d\n", pid, num)

	// get any error encountered during iteration
	err = rows.Err()
	if err != nil {
		panic(err)
	}
}

func getConnectionString(host string, port int, user string, password string, db string) string {
	return fmt.Sprintf("host=%s port=%d user=%s password=%s dbname=%s sslmode=disable",
		host, port, user, password, db)
}

func getID() int {
	mu.Lock()
	defer mu.Unlock()
	count++
	return count
}

func getEnv(key, defaultValue string) string {
	value := os.Getenv(key)
	if len(value) == 0 {
		return defaultValue
	}
	return value
}
