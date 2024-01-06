#include <iostream>
#include <unistd.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <sys/sem.h>

#define SHARED_MEMORY_KEY 1234
#define SEMAPHORE_KEY 5678

union semun {
    int val;
    struct semid_ds *buf;
    ushort *array;
};

void error(const char *msg) {
    perror(msg);
    exit(EXIT_FAILURE);
}

int create_semaphore() {
    int sem_id = semget(SEMAPHORE_KEY, 1, IPC_CREAT | 0666);
    if (sem_id == -1) {
        error("semget");
    }
    return sem_id;
}

void init_semaphore(int sem_id, int initial_value) {
    union semun arg;
    arg.val = initial_value;
    if (semctl(sem_id, 0, SETVAL, arg) == -1) {
        error("semctl");
    }
}

void semaphore_wait(int sem_id) {
    struct sembuf sops = {0, -1, 0};
    if (semop(sem_id, &sops, 1) == -1) {
        error("semop wait");
    }
}

void semaphore_signal(int sem_id) {
    struct sembuf sops = {0, 1, 0};
    if (semop(sem_id, &sops, 1) == -1) {
        error("semop signal");
    }
}

int create_shared_memory() {
    int shmid = shmget(SHARED_MEMORY_KEY, sizeof(int), IPC_CREAT | 0666);
    if (shmid == -1) {
        error("shmget");
    }
    return shmid;
}

int* attach_shared_memory(int shmid) {
    int* shared_data = (int*)shmat(shmid, nullptr, 0);
    if (*shared_data == -1) {
        error("shmat");
    }
    return shared_data;
}

void detach_shared_memory(int* shared_data) {
    if (shmdt(shared_data) == -1) {
        error("shmdt");
    }
}

int main() {
    // Создание семафора
    int sem_id = create_semaphore();
    init_semaphore(sem_id, 1);  // Инициализация семафора

    // Создание разделяемой памяти
    int shmid = create_shared_memory();
    int* shared_data = attach_shared_memory(shmid);

    pid_t child_pid = fork();

    if (child_pid == -1) {
        error("fork");
    } else if (child_pid == 0) {
        // Дочерний процесс
        semaphore_wait(sem_id);
        std::cout << "Дочерний процесс получил доступ к разделяемой памяти." << std::endl;

        // Использование разделяемой памяти
        (*shared_data)++;
        std::cout << "Дочерний процесс увеличил значение в разделяемой памяти: " << *shared_data << std::endl;

        semaphore_signal(sem_id);
        std::cout << "Дочерний процесс освободил доступ к разделяемой памяти." << std::endl;

        // Отключение от разделяемой памяти
        detach_shared_memory(shared_data);

    } else {
        // Родительский процесс
        semaphore_wait(sem_id);
        std::cout << "Родительский процесс получил доступ к разделяемой памяти." << std::endl;

        // Использование разделяемой памяти
        (*shared_data)++;
        std::cout << "Родительский процесс увеличил значение в разделяемой памяти: " << *shared_data << std::endl;

        semaphore_signal(sem_id);
        std::cout << "Родительский процесс освободил доступ к разделяемой памяти." << std::endl;

        // Ожидание завершения дочернего процесса
        wait(nullptr);

        // Отключение от разделяемой памяти
        detach_shared_memory(shared_data);

        // Удаление семафора и разделяемой памяти
        if (semctl(sem_id, 0, IPC_RMID) == -1) {
            error("semctl IPC_RMID");
        }
        if (shmctl(shmid, IPC_RMID, nullptr) == -1) {
            error("shmctl IPC_RMID");
        }
    }

    return 0;
}

