#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <unistd.h>
#define FILE_LENGTH 0x100
int main (int argc, char* const argv[])
{
	int fd;
	FILE* file_memory;
	int integer;
	/* Открыть файл. */
	fd = open (argv[1], O_RDWR, S_IRUSR | S_IWUSR);
	/* Отобразить файл в память.  */
	file_memory = mmap (0, FILE_LENGTH, PROT_READ | PROT_WRITE,
			MAP_SHARED, fd, 0);
	close (fd);
	/* Чтение целого  числа, распечатка и умножение на 2.  */
	fscanf (file_memory, "%i", &integer);
	printf ("значение: %d\n", integer);
	sprintf ((char*) file_memory, "%d\n", 2 * integer);
	/* Освобождение памяти. */ 
	munmap (file_memory, FILE_LENGTH);
	return 0;
}
